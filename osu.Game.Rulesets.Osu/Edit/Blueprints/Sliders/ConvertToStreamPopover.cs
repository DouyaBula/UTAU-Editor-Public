// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Threading;
using osu.Framework.Utils;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Components;
using osu.Game.Screens.Edit.Components.RadioButtons;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders
{
    internal partial class ConvertToStreamPopover : EditorPreviewPopover
    {
        private const double opacity_restore_delay = 500;

        private readonly Slider slider;
        private readonly SliderSelectionBlueprint selectionBlueprint;
        private readonly List<HitCircle> previewCircles = new List<HitCircle>();

        private FormSliderBar<double> accelerationInput = null!;
        private RoundedButton okButton = null!;
        private OsuSpriteText spacingRangeText = null!;
        private EditorRadioButton normalModeButton = null!;
        private EditorRadioButton mirrorModeButton = null!;

        private Bindable<bool> mirrorMode = null!;

        private ScheduledDelegate? opacityRestoreDelegate;

        [Resolved]
        private BindableBeatDivisor beatDivisor { get; set; } = null!;

        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; } = null!;

        public ConvertToStreamPopover(Slider slider, SliderSelectionBlueprint selectionBlueprint)
            : base(false)
        {
            this.slider = slider;
            this.selectionBlueprint = selectionBlueprint;
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            mirrorMode = config.GetBindable<bool>(OsuSetting.EditorConvertToStreamMirror);

            normalModeButton = new EditorRadioButton("Normal", () => mirrorMode.Value = false);
            mirrorModeButton = new EditorRadioButton("Mirror", () => mirrorMode.Value = true);

            AllowableAnchors = new[] { Anchor.CentreLeft, Anchor.CentreRight };

            Child = new FillFlowContainer
            {
                Width = 320,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(10),
                Padding = new MarginPadding(20),
                Children = new Drawable[]
                {
                    accelerationInput = new FormSliderBar<double>
                    {
                        Caption = "Acceleration",
                        Current = config.GetBindable<double>(OsuSetting.EditorConvertToStreamAcceleration),
                        KeyboardStep = 0.01f,
                        LabelFormat = value => $"{value:0.00}",
                        TooltipFormat = value => $"{value:0.00}",
                        TabbableContentContainer = this
                    },
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(10),
                        Children = new Drawable[]
                        {
                            new EditorRadioButtonCollection
                            {
                                RelativeSizeAxes = Axes.X,
                                Width = 0.62f,
                                Items = new[] { normalModeButton, mirrorModeButton }
                            },
                            new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                Width = 0.34f,
                                Height = 70,
                                Child = spacingRangeText = new OsuSpriteText
                                {
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    Font = OsuFont.Default.With(size: 16),
                                    Text = "1x -> 1x"
                                }
                            },
                        }
                    },
                    new EditorToolActionRow(okButton = new RoundedButton { Action = commit }, Hide)
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            BeginPreview();
            selectionBlueprint.SetConvertToStreamPreviewHidden(true);

            accelerationInput.Current.BindValueChanged(_ => schedulePreviewUpdate(true));
            mirrorMode.BindValueChanged(value =>
            {
                if (value.NewValue)
                    mirrorModeButton.Select();
                else
                    normalModeButton.Select();

                schedulePreviewUpdate(false);
            }, true);

            tryCreatePreview();
        }

        private void schedulePreviewUpdate(bool dimPopover)
        {
            if (!PreviewClosed)
            {
                if (dimPopover)
                    dimDuringAdjustment();

                Scheduler.AddOnce(tryCreatePreview);
            }
        }

        private void dimDuringAdjustment()
        {
            opacityRestoreDelegate?.Cancel();

            this.FadeTo(0.22f, 80, Easing.OutQuint);

            opacityRestoreDelegate = Scheduler.AddDelayed(() =>
            {
                if (!PreviewClosed && State.Value == Visibility.Visible)
                    this.FadeTo(1, 160, Easing.OutQuint);
            }, opacity_restore_delay);
        }

        private void tryCreatePreview()
        {
            if (!BeginPreview())
                return;

            ReplacePreviewObjects(previewCircles, createStreamCircles(accelerationInput.Current.Value, mirrorMode.Value));
            okButton.Enabled.Value = previewCircles.Count > 0;
        }

        private IEnumerable<HitCircle> createStreamCircles(double acceleration, bool mirror)
        {
            if (Precision.AlmostEquals(slider.Duration, 0))
            {
                spacingRangeText.Text = "1x -> 1x";
                yield break;
            }

            TimingControlPoint timingPoint = editorBeatmap.ControlPointInfo.TimingPointAt(slider.StartTime);
            double streamSpacing = timingPoint.BeatLength / beatDivisor.Value;

            int i = 0;
            double time = slider.StartTime;
            var transformedPositions = new List<double>();

            while (!Precision.DefinitelyBigger(time, slider.GetEndTime(), 1))
            {
                double linearProgress = Math.Clamp((time - slider.StartTime) / slider.Duration, 0, 1);
                double transformedProgress = applyAcceleration(linearProgress, acceleration, mirror);
                double positionWithRepeats = transformedProgress * slider.SpanCount();
                transformedPositions.Add(positionWithRepeats);

                double pathPosition = positionWithRepeats - (int)positionWithRepeats;

                // Every second span is in the reverse direction.
                if (positionWithRepeats % 2 >= 1)
                    pathPosition = 1 - pathPosition;

                Vector2 position = slider.Position + slider.Path.PositionAt(pathPosition);

                yield return new HitCircle
                {
                    StartTime = time,
                    Position = position,
                    NewCombo = i == 0 && slider.NewCombo,
                    Samples = slider.HeadCircle.Samples.Select(s => s.With()).ToList()
                };

                i += 1;
                time = slider.StartTime + i * streamSpacing;
            }

            updateSpacingRange(transformedPositions);
        }

        private void updateSpacingRange(IReadOnlyList<double> transformedPositions)
        {
            if (transformedPositions.Count < 2)
            {
                spacingRangeText.Text = "1x -> 1x";
                return;
            }

            double averageDelta = slider.SpanCount() / (double)(transformedPositions.Count - 1);
            var ratios = new List<double>();

            for (int i = 1; i < transformedPositions.Count; i++)
                ratios.Add(Math.Abs(transformedPositions[i] - transformedPositions[i - 1]) / averageDelta);

            spacingRangeText.Text = $"{ratios.Min():0.##}x -> {ratios.Max():0.##}x";
        }

        private static double applyAcceleration(double progress, double acceleration, bool mirror)
        {
            progress = Math.Clamp(progress, 0, 1);

            double strength = Math.Abs(acceleration);

            if (strength < 0.0001)
                return progress;

            double exponent = 1 + strength * 4;

            if (!mirror)
                return acceleration >= 0 ? easeIn(progress, exponent) : easeOut(progress, exponent);

            return acceleration >= 0 ? easeInOut(progress, exponent) : easeOutIn(progress, exponent);
        }

        private static double easeIn(double progress, double exponent) => Math.Pow(progress, exponent);

        private static double easeOut(double progress, double exponent) => 1 - Math.Pow(1 - progress, exponent);

        private static double easeInOut(double progress, double exponent)
        {
            if (progress < 0.5)
                return 0.5 * easeIn(progress * 2, exponent);

            return 1 - 0.5 * easeIn((1 - progress) * 2, exponent);
        }

        private static double easeOutIn(double progress, double exponent)
        {
            if (progress < 0.5)
                return 0.5 * easeOut(progress * 2, exponent);

            return 0.5 + 0.5 * easeIn(progress * 2 - 1, exponent);
        }

        private void commit()
        {
            tryCreatePreview();

            if (previewCircles.Count == 0)
                return;

            editorBeatmap.Remove(slider);
            previewCircles.Clear();

            opacityRestoreDelegate?.Cancel();
            CommitPreview();
        }

        protected override void RestorePreview()
        {
            opacityRestoreDelegate?.Cancel();
            selectionBlueprint.SetConvertToStreamPreviewHidden(false);
            editorBeatmap.RemoveRange(previewCircles);
            previewCircles.Clear();
        }
    }
}
