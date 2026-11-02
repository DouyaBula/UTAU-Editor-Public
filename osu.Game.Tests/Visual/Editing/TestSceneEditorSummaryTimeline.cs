// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Lines;
using osu.Framework.Testing;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Configuration;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Components.Timelines.Summary;
using osu.Game.Screens.Edit.Components.Timelines.Summary.Parts;
using osu.Game.Screens.Edit.Compose.Components.Timeline;
using osuTK;

namespace osu.Game.Tests.Visual.Editing
{
    [TestFixture]
    public partial class TestSceneEditorSummaryTimeline : EditorClockTestScene
    {
        private readonly Bindable<IReadOnlyList<DifficultyRatingSample>> difficultySamples = new Bindable<IReadOnlyList<DifficultyRatingSample>>(
            new[]
            {
                new DifficultyRatingSample(0, 0.1, "Aim"),
                new DifficultyRatingSample(60000, 0.75, "Aim"),
                new DifficultyRatingSample(120000, 0.25, "Aim")
            });

        [Cached(typeof(EditorBeatmap))]
        private readonly EditorBeatmap editorBeatmap;

        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        public TestSceneEditorSummaryTimeline()
        {
            var beatmap = CreateBeatmap(new OsuRuleset().RulesetInfo);

            beatmap.ControlPointInfo.Add(100000, new TimingControlPoint { BeatLength = 100 });
            beatmap.ControlPointInfo.Add(50000, new DifficultyControlPoint { SliderVelocity = 2 });
            beatmap.ControlPointInfo.Add(80000, new EffectControlPoint { KiaiMode = true });
            beatmap.ControlPointInfo.Add(110000, new EffectControlPoint { KiaiMode = false });
            beatmap.Bookmarks = new[] { 75000, 125000 };
            beatmap.Breaks.Add(new ManualBreakPeriod(90000, 120000));

            editorBeatmap = new EditorBeatmap(beatmap);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AddStep("create timeline", () =>
            {
                config.SetValue(OsuSetting.EditorDifficultyTimelineOnTop, true);

                // required for track
                Beatmap.Value = CreateWorkingBeatmap(editorBeatmap.PlayableBeatmap);

                Add(new SummaryTimeline(difficultySamples)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(500, 50)
                });
            });

            AddUntilStep("wait for summary difficulty curve", () => this.ChildrenOfType<DifficultyRatingTimelineOverlay>().SingleOrDefault()?.IsLoaded == true);
            AddUntilStep("wait for curve vertices", () => this.ChildrenOfType<DifficultyRatingTimelineOverlay>().SingleOrDefault()?.RenderedSampleCount == 3);
            AddAssert("difficulty curve is on summary timeline", () => this.ChildrenOfType<SummaryTimeline>().Single().ChildrenOfType<DifficultyRatingTimelineOverlay>().Single().IsLoaded);
            AddAssert("difficulty curve has a visible size", () =>
            {
                var path = this.ChildrenOfType<SmoothPath>().Single();
                return path.DrawWidth > 0 && path.DrawHeight > 0;
            });
            AddAssert("difficulty curve renders all sampled points", () => this.ChildrenOfType<DifficultyRatingTimelineOverlay>().Single().RenderedSampleCount == 3);
            AddAssert("difficulty curve defaults above timeline markers", () => this.ChildrenOfType<DifficultyRatingTimelineOverlay>().Single().Depth < this.ChildrenOfType<KiaiPart>().Single().Depth);
            AddStep("send difficulty curve behind timeline markers", () => config.SetValue(OsuSetting.EditorDifficultyTimelineOnTop, false));
            AddAssert("difficulty curve render order is configurable", () => this.ChildrenOfType<DifficultyRatingTimelineOverlay>().Single().Depth == 0);
            AddStep("restore difficulty curve above timeline markers", () => config.SetValue(OsuSetting.EditorDifficultyTimelineOnTop, true));
            AddAssert("difficulty curve returns to top layer", () => this.ChildrenOfType<DifficultyRatingTimelineOverlay>().Single().Depth < this.ChildrenOfType<KiaiPart>().Single().Depth);
        }
    }
}
