// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Localisation;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Osu.Edit.Curves;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Components;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit
{
    public partial class UTAUEditorToolboxGroup : EditorToolboxGroup
    {
        private readonly OsuHitObjectComposer composer;
        private readonly List<Drawable> pendingFeatureButtons = new List<Drawable>();
        private FillFlowContainer featureButtons = null!;

        public UTAUEditorToolboxGroup(OsuHitObjectComposer composer)
            : base("UTAU Editor")
        {
            this.composer = composer;
        }

        [BackgroundDependencyLoader]
        private void load(EditorBeatmap editorBeatmap, OsuConfigManager config)
        {
            Bindable<float> valueA = config.GetBindable<float>(OsuSetting.EditorApproachRateToggleValueA);
            Bindable<float> valueB = config.GetBindable<float>(OsuSetting.EditorApproachRateToggleValueB);
            Bindable<bool> allowOutOfBoundsPlacement = config.GetBindable<bool>(OsuSetting.EditorAllowOutOfBoundsPlacement);

            Child = featureButtons = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(5),
            };

            AddFeatureButton(new EditorToolButton("Approach Rate",
                () => new SpriteIcon { Icon = FontAwesome.Solid.Eye },
                () => new ApproachRateTogglePopover(),
                () => EditorApproachRateToggle.Toggle(editorBeatmap, valueA.Value, valueB.Value)));

            AddFeatureButton(new EditorToolButton(EditorStrings.CanvasZoom,
                () => new SpriteIcon { Icon = FontAwesome.Solid.SearchPlus },
                () => new CanvasZoomPopover()));

            AddFeatureButton(new EditorToolButton("Slider detail",
                () => new SpriteIcon { Icon = FontAwesome.Solid.Cog },
                () => new SliderDetailPopover()));

            AddFeatureButton(new EditorToolButton("Slider curves",
                () => new SpriteIcon { Icon = FontAwesome.Solid.BezierCurve },
                () => new CurveToolsPopover()));

            AddFeatureButton(new EditorToolButton("Difficulty display",
                () => new SpriteIcon { Icon = FontAwesome.Solid.ChartPie },
                () => new DifficultyVisualizationPopover(composer)));

            AddFeatureButton(new EditorToolButton(EditorStrings.HitsoundTools,
                () => new SpriteIcon { Icon = FontAwesome.Solid.Music },
                () => new HitsoundToolsPopover()));

            var outOfBoundsPlacementButton = new EditorToolButton(EditorStrings.OutOfBoundsPlacement,
                () => new SpriteIcon { Icon = FontAwesome.Solid.ExpandArrowsAlt },
                () => null,
                () => allowOutOfBoundsPlacement.Value = !allowOutOfBoundsPlacement.Value);
            outOfBoundsPlacementButton.Selected.Value = allowOutOfBoundsPlacement.Value;
            AddFeatureButton(outOfBoundsPlacementButton);

            if (pendingFeatureButtons.Count > 0)
            {
                featureButtons.AddRange(pendingFeatureButtons);
                pendingFeatureButtons.Clear();
            }
        }

        public void AddFeatureButton(Drawable featureButton)
        {
            if (featureButtons == null)
            {
                pendingFeatureButtons.Add(featureButton);
                return;
            }

            featureButtons.Add(featureButton);
        }
    }
}
