// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Screens.Edit.Components;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit
{
    public partial class DifficultyVisualizationPopover : EditorPopover
    {
        private readonly OsuHitObjectComposer composer;
        private FormButton updateButton = null!;

        public DifficultyVisualizationPopover(OsuHitObjectComposer composer)
            : base(false)
        {
            this.composer = composer;
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            AllowableAnchors = new[] { Anchor.CentreLeft, Anchor.CentreRight };

            updateButton = new FormButton
            {
                Caption = "Update difficulty now",
                ButtonText = "Update",
                Action = composer.UpdateDifficulty,
            };

            updateButton.Enabled.Value = !composer.DifficultyCalculationInProgress.Value;
            composer.DifficultyCalculationInProgress.ValueChanged += onCalculationStateChanged;

            Child = new FillFlowContainer
            {
                Width = 320,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(10),
                Padding = new MarginPadding(16),
                Children = new Drawable[]
                {
                    new FormCheckBox { Caption = "Auto-update difficulty on save (Ctrl+S)", Current = config.GetBindable<bool>(OsuSetting.EditorDifficultyUpdateOnSave) },
                    updateButton,
                    new EditorToolPages(
                        ("Strain curve", new Drawable[]
                        {
                            new FormCheckBox { Caption = "Show timeline difficulty curve", Current = config.GetBindable<bool>(OsuSetting.EditorDifficultyTimelineVisible) },
                            new FormCheckBox { Caption = "Render curve above timeline markers", Current = config.GetBindable<bool>(OsuSetting.EditorDifficultyTimelineOnTop) },
                            new FormSliderBar<float> { Caption = "Curve opacity", Current = config.GetBindable<float>(OsuSetting.EditorDifficultyTimelineOpacity), DisplayAsPercentage = true, TabbableContentContainer = this },
                            new FormSliderBar<float> { Caption = "Curve height", Current = config.GetBindable<float>(OsuSetting.EditorDifficultyTimelineHeight), DisplayAsPercentage = true, TabbableContentContainer = this },
                            new FormSliderBar<float> { Caption = "Curve sample interval (ms)", Current = config.GetBindable<float>(OsuSetting.EditorDifficultyTimelineInterval), KeyboardStep = 50, TabbableContentContainer = this },
                        }),
                        ("Overall star rating", new Drawable[]
                        {
                            new FormCheckBox { Caption = "Show overall star rating", Current = config.GetBindable<bool>(OsuSetting.EditorDifficultyRatingVisible) },
                            new FormCheckBox { Caption = "Place star rating at upper left", Current = config.GetBindable<bool>(OsuSetting.EditorDifficultyRatingOnLeft) },
                        }))
                }
            };
        }

        private void onCalculationStateChanged(ValueChangedEvent<bool> e) => updateButton.Enabled.Value = !e.NewValue;

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing && composer != null)
                composer.DifficultyCalculationInProgress.ValueChanged -= onCalculationStateChanged;

            base.Dispose(isDisposing);
        }
    }
}
