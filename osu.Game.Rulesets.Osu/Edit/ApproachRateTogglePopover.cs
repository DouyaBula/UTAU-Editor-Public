// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Globalization;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Components;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit
{
    public partial class ApproachRateTogglePopover : EditorPopover
    {
        private Bindable<float> valueA = null!;
        private Bindable<float> valueB = null!;

        private OsuSpriteText currentApproachRateText = null!;

        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            valueA = config.GetBindable<float>(OsuSetting.EditorApproachRateToggleValueA);
            valueB = config.GetBindable<float>(OsuSetting.EditorApproachRateToggleValueB);

            AllowableAnchors = new[] { Anchor.CentreLeft, Anchor.CentreRight };

            Child = new FillFlowContainer
            {
                Width = 220,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(5),
                Children = new Drawable[]
                {
                    currentApproachRateText = new OsuSpriteText
                    {
                        RelativeSizeAxes = Axes.X,
                    },
                    new RoundedButton
                    {
                        RelativeSizeAxes = Axes.X,
                        Text = "Toggle approach rate",
                        Action = toggleApproachRate,
                    },
                    new FormSliderBar<float>
                    {
                        Caption = "Value A",
                        Current = valueA,
                        KeyboardStep = 0.1f,
                        TabbableContentContainer = this,
                    },
                    new FormSliderBar<float>
                    {
                        Caption = "Value B",
                        Current = valueB,
                        KeyboardStep = 0.1f,
                        TabbableContentContainer = this,
                    },
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            valueA.BindValueChanged(_ => updateCurrentApproachRateText());
            valueB.BindValueChanged(_ => updateCurrentApproachRateText());
            updateCurrentApproachRateText();
        }

        protected override void Update()
        {
            base.Update();

            updateCurrentApproachRateText();
        }

        private void toggleApproachRate()
        {
            EditorApproachRateToggle.Toggle(editorBeatmap, valueA.Value, valueB.Value);
            updateCurrentApproachRateText();
        }

        private void updateCurrentApproachRateText()
        {
            currentApproachRateText.Text = $@"Current AR: {editorBeatmap.Difficulty.ApproachRate.ToString("0.0", CultureInfo.InvariantCulture)}";
        }
    }
}
