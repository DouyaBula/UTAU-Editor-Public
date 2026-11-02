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
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Components;
using osu.Game.Screens.Edit.Components.RadioButtons;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit
{
    internal partial class RandomPerturbSelectionPopover : EditorPreviewPopover
    {
        private const double opacity_restore_delay = 500;

        private readonly OsuHitObject[] hitObjects;
        private readonly OsuSelectionHandler selectionHandler;
        private readonly Dictionary<OsuHitObject, Vector2> originalPositions;

        private FormSliderBar<double> strengthInput = null!;
        private FormSliderBar<float> seedInput = null!;
        private EditorRadioButton circleModeButton = null!;
        private EditorRadioButton squareModeButton = null!;
        private EditorRadioButton horizontalModeButton = null!;
        private EditorRadioButton verticalModeButton = null!;

        private Bindable<string> mode = null!;

        private ScheduledDelegate? opacityRestoreDelegate;

        public RandomPerturbSelectionPopover(IEnumerable<OsuHitObject> hitObjects, OsuSelectionHandler selectionHandler)
            : base(false)
        {
            this.selectionHandler = selectionHandler;
            this.hitObjects = hitObjects.OrderBy(h => h.StartTime).ThenBy(h => h.Position.X).ThenBy(h => h.Position.Y).ToArray();
            originalPositions = this.hitObjects.ToDictionary(h => h, h => h.Position);
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            mode = config.GetBindable<string>(OsuSetting.EditorRandomPerturbMode);

            circleModeButton = new EditorRadioButton("Circle", () => mode.Value = "Circle");
            squareModeButton = new EditorRadioButton("Square", () => mode.Value = "Square");
            horizontalModeButton = new EditorRadioButton("Horizontal", () => mode.Value = "Horizontal");
            verticalModeButton = new EditorRadioButton("Vertical", () => mode.Value = "Vertical");

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
                    new EditorRadioButtonCollection
                    {
                        RelativeSizeAxes = Axes.X,
                        Items = new[] { circleModeButton, squareModeButton, horizontalModeButton, verticalModeButton }
                    },
                    strengthInput = new FormSliderBar<double>
                    {
                        Caption = "Strength",
                        Current = config.GetBindable<double>(OsuSetting.EditorRandomPerturbStrength),
                        KeyboardStep = 0.5f,
                        LabelFormat = value => $"{value:0.#}px",
                        TooltipFormat = value => $"{value:0.#}px",
                        TabbableContentContainer = this
                    },
                    seedInput = new FormSliderBar<float>
                    {
                        Caption = "Seed",
                        Current = config.GetBindable<float>(OsuSetting.EditorRandomPerturbSeed),
                        KeyboardStep = 1f,
                        LabelFormat = value => $"{value:0}",
                        TooltipFormat = value => $"{value:0}",
                        TabbableContentContainer = this
                    },
                    new EditorToolActionRow(new RoundedButton { Action = commit }, Hide)
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            strengthInput.Current.BindValueChanged(_ => scheduleApplyPreview(true));
            seedInput.Current.BindValueChanged(_ => scheduleApplyPreview(true));
            mode.BindValueChanged(value =>
            {
                selectModeButton(value.NewValue);
                scheduleApplyPreview(false);
            }, true);

            applyPreview();
        }

        private void scheduleApplyPreview(bool dimPopover)
        {
            if (!PreviewClosed)
            {
                if (dimPopover)
                    dimDuringAdjustment();

                Scheduler.AddOnce(applyPreview);
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

        private void applyPreview()
        {
            if (!BeginPreview())
                return;

            string currentMode = normaliseMode(mode.Value);
            double strength = strengthInput.Current.Value;
            int seed = (int)Math.Round(seedInput.Current.Value);

            for (int i = 0; i < hitObjects.Length; i++)
                hitObjects[i].Position = originalPositions[hitObjects[i]] + createOffset(currentMode, strength, seed, i);

            selectionHandler.UpdateAfterRandomPerturbPreview();
        }

        private void restoreOriginalPositions()
        {
            foreach (var hitObject in hitObjects)
                hitObject.Position = originalPositions[hitObject];

            selectionHandler.UpdateAfterRandomPerturbPreview();
        }

        private void selectModeButton(string value)
        {
            switch (normaliseMode(value))
            {
                case "Square":
                    squareModeButton.Select();
                    break;

                case "Horizontal":
                    horizontalModeButton.Select();
                    break;

                case "Vertical":
                    verticalModeButton.Select();
                    break;

                default:
                    circleModeButton.Select();
                    break;
            }
        }

        private static string normaliseMode(string? value) => value switch
        {
            "Square" => "Square",
            "Horizontal" => "Horizontal",
            "Vertical" => "Vertical",
            _ => "Circle"
        };

        private static Vector2 createOffset(string mode, double strength, int seed, int index)
        {
            if (strength <= 0)
                return Vector2.Zero;

            var random = new Random(unchecked(seed * 73856093 ^ index * 19349663));
            float amount = (float)strength;

            switch (mode)
            {
                case "Square":
                    return new Vector2(nextSigned(random), nextSigned(random)) * amount;

                case "Horizontal":
                    return new Vector2(nextSigned(random) * amount, 0);

                case "Vertical":
                    return new Vector2(0, nextSigned(random) * amount);

                default:
                    double angle = random.NextDouble() * Math.PI * 2;
                    double radius = Math.Sqrt(random.NextDouble()) * amount;
                    return new Vector2((float)(Math.Cos(angle) * radius), (float)(Math.Sin(angle) * radius));
            }
        }

        private static float nextSigned(Random random) => (float)(random.NextDouble() * 2 - 1);

        private void commit()
        {
            applyPreview();

            opacityRestoreDelegate?.Cancel();
            CommitPreview();
        }

        protected override void RestorePreview()
        {
            opacityRestoreDelegate?.Cancel();
            restoreOriginalPositions();
        }
    }
}
