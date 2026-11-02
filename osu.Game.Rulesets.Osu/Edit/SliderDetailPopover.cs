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
    public partial class SliderDetailPopover : EditorPopover
    {
        private Bindable<bool> detailedMode = null!;
        private Bindable<float> detailedLimit = null!;
        private Bindable<float> roughLineMaxPoints = null!;

        public SliderDetailPopover()
            : base(false)
        {
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            detailedMode = config.GetBindable<bool>(OsuSetting.EditorSliderDetailedControlPointMode);
            detailedLimit = config.GetBindable<float>(OsuSetting.EditorSliderDetailedControlPointLimit);
            roughLineMaxPoints = config.GetBindable<float>(OsuSetting.EditorSliderRoughControlLineMaxPoints);

            AllowableAnchors = new[] { Anchor.CentreLeft, Anchor.CentreRight };

            Child = new FillFlowContainer
            {
                Width = 340,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(10),
                Padding = new MarginPadding(20),
                Children = new Drawable[]
                {
                    new FormCheckBox
                    {
                        Caption = "Detailed high-density sliders",
                        HintText = "When enabled, selected high-control-point sliders show editable control points instead of the rough preview.",
                        Current = detailedMode,
                    },
                    new FormSliderBar<float>
                    {
                        Caption = "Detailed point limit",
                        HintText = "Sliders at or below this control point count use editable detailed mode. Set to 0 to default all sliders to rough mode.",
                        Current = detailedLimit,
                        KeyboardStep = 64,
                        LabelFormat = value => $"{value:0} points",
                        TooltipFormat = value => $"{value:0} points",
                        TabbableContentContainer = this,
                    },
                    new FormSliderBar<float>
                    {
                        Caption = "Rough line samples",
                        HintText = "Maximum control points used to draw the rough non-editable control line.",
                        Current = roughLineMaxPoints,
                        KeyboardStep = 16,
                        LabelFormat = value => $"{value:0} points",
                        TooltipFormat = value => $"{value:0} points",
                        TabbableContentContainer = this,
                    },
                }
            };
        }
    }
}
