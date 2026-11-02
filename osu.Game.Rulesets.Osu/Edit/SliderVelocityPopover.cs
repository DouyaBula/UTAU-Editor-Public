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
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Components;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit
{
    internal partial class SliderVelocityPopover : EditorPreviewPopover
    {
        private const double opacity_restore_delay = 500;

        private readonly OriginalSliderState[] originalStates;

        private readonly Bindable<SliderVelocityCurve> curve = new Bindable<SliderVelocityCurve>();

        private FormSliderBar<double> intensityInput = null!;
        private FormSliderBar<double> velocityInput = null!;
        private FormSliderBar<double> powerInput = null!;
        private FormSliderBar<float> frequencyInput = null!;
        private FormSliderBar<double> overshootInput = null!;
        private FormSliderBar<float> sampleCountInput = null!;
        private FormSliderBar<double> maxDetourLengthInput = null!;
        private EditorToolStatus resultText = null!;
        private RoundedButton okButton = null!;

        private Bindable<string> curveConfig = null!;
        private ScheduledDelegate? opacityRestoreDelegate;

        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; } = null!;

        public SliderVelocityPopover(IEnumerable<Slider> sliders)
            : base(false)
        {
            originalStates = sliders.Distinct().Select(slider => new OriginalSliderState(slider)).ToArray();
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            curveConfig = config.GetBindable<string>(OsuSetting.EditorSliderVelocityCurve);
            curve.Value = parseCurve(curveConfig.Value);

            AllowableAnchors = new[] { Anchor.CentreLeft, Anchor.CentreRight };

            Child = new FillFlowContainer
            {
                Width = 560,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(8),
                Padding = new MarginPadding(20),
                Children = new Drawable[]
                {
                    new FormEnumDropdown<SliderVelocityCurve>
                    {
                        Caption = "Curve",
                        Current = curve
                    },
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(16),
                        Children = new Drawable[]
                        {
                            new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                Width = 0.48f,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                                Spacing = new Vector2(8),
                                Children = new Drawable[]
                                {
                                    intensityInput = new FormSliderBar<double>
                                    {
                                        Caption = "Intensity",
                                        HintText = "Blends the selected curve with constant speed. Increase the fixed SV multiplier if the applied intensity is capped.",
                                        Current = config.GetBindable<double>(OsuSetting.EditorSliderVelocityIntensity),
                                        KeyboardStep = 0.05f,
                                        LabelFormat = value => $"{value:P0}",
                                        TooltipFormat = value => $"{value:P0}",
                                        TabbableContentContainer = this
                                    },
                                    velocityInput = new FormSliderBar<double>
                                    {
                                        Caption = "Fixed SV multiplier",
                                        HintText = "Adds speed headroom used for hidden backtracking while preserving duration.",
                                        Current = config.GetBindable<double>(OsuSetting.EditorSliderVelocityMultiplier),
                                        KeyboardStep = 0.1f,
                                        LabelFormat = value => $"{value:0.0#}x",
                                        TooltipFormat = value => $"{value:0.0#}x",
                                        TabbableContentContainer = this
                                    },
                                    powerInput = new FormSliderBar<double>
                                    {
                                        Caption = "Power",
                                        HintText = "Controls accelerate, decelerate, and bounce curves.",
                                        Current = config.GetBindable<double>(OsuSetting.EditorSliderVelocityPower),
                                        KeyboardStep = 0.25f,
                                        LabelFormat = value => $"{value:0.##}",
                                        TooltipFormat = value => $"{value:0.##}",
                                        TabbableContentContainer = this
                                    },
                                    frequencyInput = new FormSliderBar<float>
                                    {
                                        Caption = "Frequency",
                                        HintText = "Controls wave, bounce, and elastic oscillations.",
                                        Current = config.GetBindable<float>(OsuSetting.EditorSliderVelocityFrequency),
                                        KeyboardStep = 1,
                                        LabelFormat = value => $"{value:0}",
                                        TooltipFormat = value => $"{value:0}",
                                        TabbableContentContainer = this
                                    }
                                }
                            },
                            new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                Width = 0.48f,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                                Spacing = new Vector2(8),
                                Children = new Drawable[]
                                {
                                    overshootInput = new FormSliderBar<double>
                                    {
                                        Caption = "Overshoot",
                                        HintText = "Controls wave amplitude and elastic or back overshoot.",
                                        Current = config.GetBindable<double>(OsuSetting.EditorSliderVelocityOvershoot),
                                        KeyboardStep = 0.05f,
                                        LabelFormat = value => $"{value:P0}",
                                        TooltipFormat = value => $"{value:P0}",
                                        TabbableContentContainer = this
                                    },
                                    sampleCountInput = new FormSliderBar<float>
                                    {
                                        Caption = "Curve samples",
                                        HintText = "Higher values follow the original curve more accurately but add more control points.",
                                        Current = config.GetBindable<float>(OsuSetting.EditorSliderVelocitySampleCount),
                                        KeyboardStep = 8,
                                        LabelFormat = value => $"{value:0}",
                                        TooltipFormat = value => $"{value:0}",
                                        TabbableContentContainer = this
                                    },
                                    maxDetourLengthInput = new FormSliderBar<double>
                                    {
                                        Caption = "Maximum hidden segment",
                                        HintText = "Limits each backtracking branch so it remains inside the slider body.",
                                        Current = config.GetBindable<double>(OsuSetting.EditorSliderVelocityMaxDetourLength),
                                        KeyboardStep = 0.5f,
                                        LabelFormat = value => $"{value:0.#}px",
                                        TooltipFormat = value => $"{value:0.#}px",
                                        TabbableContentContainer = this
                                    }
                                }
                            }
                        }
                    },
                    resultText = new EditorToolStatus { Text = "Preparing preview..." },
                    new EditorToolActionRow(okButton = new RoundedButton { Action = commit }, Hide)
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            curve.BindValueChanged(value =>
            {
                curveConfig.Value = value.NewValue.ToString();
                schedulePreviewUpdate(false);
            });

            intensityInput.Current.BindValueChanged(_ => schedulePreviewUpdate(true));
            velocityInput.Current.BindValueChanged(_ => schedulePreviewUpdate(true));
            powerInput.Current.BindValueChanged(_ => schedulePreviewUpdate(true));
            frequencyInput.Current.BindValueChanged(_ => schedulePreviewUpdate(true));
            overshootInput.Current.BindValueChanged(_ => schedulePreviewUpdate(true));
            sampleCountInput.Current.BindValueChanged(_ => schedulePreviewUpdate(true));
            maxDetourLengthInput.Current.BindValueChanged(_ => schedulePreviewUpdate(true));

            applyPreview();
        }

        private void schedulePreviewUpdate(bool dimPopover)
        {
            if (PreviewClosed)
                return;

            if (dimPopover)
                dimDuringAdjustment();

            Scheduler.AddOnce(applyPreview);
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

        private void applyPreview()
        {
            if (!BeginPreview())
                return;

            foreach (OriginalSliderState state in originalStates)
                state.Restore(editorBeatmap);

            if (originalStates.Length == 0)
            {
                resultText.Text = "Select at least one slider.";
                okButton.Enabled.Value = false;
                return;
            }

            var transformations = new List<(OriginalSliderState State, SliderVelocityTransformation Transformation)>();

            foreach (OriginalSliderState state in originalStates)
            {
                double maximumAllowedMultiplier = Math.Max(1, 10 / state.SliderVelocityMultiplier);
                double requestedMultiplier = Math.Min(velocityInput.Current.Value, maximumAllowedMultiplier);

                SliderVelocityTransformation? transformation = SliderVelocityTransformer.Create(
                    state.Path,
                    state.Path.Distance,
                    requestedMultiplier,
                    curve.Value,
                    intensityInput.Current.Value,
                    powerInput.Current.Value,
                    (int)Math.Round(frequencyInput.Current.Value),
                    overshootInput.Current.Value,
                    (int)Math.Round(sampleCountInput.Current.Value),
                    maxDetourLengthInput.Current.Value);

                if (transformation == null)
                {
                    resultText.Text = "Unable to generate within the control point limit.";
                    okButton.Enabled.Value = false;
                    return;
                }

                transformations.Add((state, transformation));
            }

            foreach ((OriginalSliderState state, SliderVelocityTransformation transformation) in transformations)
            {
                state.Slider.Path = transformation.Path;
                state.Slider.SliderVelocityMultiplier = state.SliderVelocityMultiplier * transformation.VelocityMultiplier;
                editorBeatmap.Update(state.Slider);
            }

            double minimumIntensity = transformations.Min(result => result.Transformation.AppliedIntensity);
            double minimumMultiplier = transformations.Min(result => result.Transformation.VelocityMultiplier);
            double maximumMultiplier = transformations.Max(result => result.Transformation.VelocityMultiplier);
            int totalControlPoints = transformations.Sum(result => result.Transformation.ControlPointCount);

            string multiplierText = Precision.AlmostEquals(minimumMultiplier, maximumMultiplier, 0.001)
                ? $"{minimumMultiplier:0.0#}x"
                : $"{minimumMultiplier:0.0#}x - {maximumMultiplier:0.0#}x";

            resultText.Text = $"SV {multiplierText} | intensity {minimumIntensity:P0} | {totalControlPoints} points";
            okButton.Enabled.Value = true;
        }

        private void commit()
        {
            applyPreview();

            if (!okButton.Enabled.Value)
                return;

            opacityRestoreDelegate?.Cancel();
            CommitPreview();
        }

        protected override void RestorePreview()
        {
            opacityRestoreDelegate?.Cancel();

            foreach (OriginalSliderState state in originalStates)
                state.Restore(editorBeatmap);
        }

        private static SliderVelocityCurve parseCurve(string value)
            => Enum.TryParse(value, out SliderVelocityCurve result) ? result : SliderVelocityCurve.AccelerateThenDecelerate;

        private sealed class OriginalSliderState
        {
            public Slider Slider { get; }
            public SliderPath Path { get; }
            public double SliderVelocityMultiplier { get; }

            public OriginalSliderState(Slider slider)
            {
                Slider = slider;
                Path = clonePath(slider.Path);
                SliderVelocityMultiplier = slider.SliderVelocityMultiplier;
            }

            public void Restore(EditorBeatmap editorBeatmap)
            {
                Slider.Path = clonePath(Path);
                Slider.SliderVelocityMultiplier = SliderVelocityMultiplier;
                editorBeatmap.Update(Slider);
            }

            private static SliderPath clonePath(SliderPath path) =>
                new SliderPath(
                    path.ControlPoints.Select(point => new PathControlPoint(point.Position, point.Type)).ToArray(),
                    path.ExpectedDistance.Value);
        }
    }
}
