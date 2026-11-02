// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Configuration;
using osu.Game.Overlays;
using osu.Game.Screens.Edit.Components.Timelines.Summary.Parts;
using osu.Game.Screens.Edit.Compose.Components.Timeline;
using osuTK;

namespace osu.Game.Screens.Edit.Components.Timelines.Summary
{
    /// <summary>
    /// The timeline that sits at the bottom of the editor.
    /// </summary>
    public partial class SummaryTimeline : BottomBarContainer
    {
        private readonly IBindable<IReadOnlyList<DifficultyRatingSample>> difficultySamples;
        private Bindable<bool> difficultyCurveOnTop = null!;
        private DifficultyRatingTimelineOverlay difficultyCurve = null!;

        public SummaryTimeline(IBindable<IReadOnlyList<DifficultyRatingSample>> difficultySamples)
        {
            this.difficultySamples = difficultySamples;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider, OsuConfigManager config)
        {
            Background.Colour = colourProvider.Background6;
            difficultyCurveOnTop = config.GetBindable<bool>(OsuSetting.EditorDifficultyTimelineOnTop);

            Children = new Drawable[]
            {
                new Container
                {
                    Name = "centre line",
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background2,
                    Children = new Drawable[]
                    {
                        new Circle
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.Centre,
                            Size = new Vector2(5)
                        },
                        new Box
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            RelativeSizeAxes = Axes.X,
                            Height = 1,
                            EdgeSmoothness = new Vector2(0, 1),
                        },
                        new Circle
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.Centre,
                            Size = new Vector2(5)
                        },
                    }
                },
                difficultyCurve = new DifficultyRatingTimelineOverlay(difficultySamples)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                },
                new BreakPart
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                },
                new KiaiPart
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                },
                new ControlPointPart
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.BottomCentre,
                    RelativeSizeAxes = Axes.Both,
                    Height = 0.4f
                },
                new BookmarkPart
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.Both,
                    Height = 0.4f
                },
                new PreviewTimePart
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                },
                new MarkerPart { RelativeSizeAxes = Axes.Both },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            difficultyCurveOnTop.BindValueChanged(_ => updateDifficultyCurveDepth(), true);
        }

        private void updateDifficultyCurveDepth()
        {
            // Smaller depth values render in front. At depth zero the curve retains its
            // existing position in the child order, behind the other timeline parts.
            Content.ChangeChildDepth(difficultyCurve, difficultyCurveOnTop.Value ? -1 : 0);
        }
    }
}
