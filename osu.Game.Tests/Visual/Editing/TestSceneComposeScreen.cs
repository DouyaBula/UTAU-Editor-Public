// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Testing;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Edit;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Components;
using osu.Game.Screens.Edit.Compose;
using osu.Game.Screens.Edit.Compose.Components.Timeline;
using osu.Game.Skinning;

namespace osu.Game.Tests.Visual.Editing
{
    [TestFixture]
    public partial class TestSceneComposeScreen : EditorClockTestScene
    {
        private EditorBeatmap editorBeatmap = null!;

        [Cached]
        private EditorClipboard clipboard = new EditorClipboard();

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("setup compose screen", () =>
            {
                var beatmap = new OsuBeatmap
                {
                    BeatmapInfo = { Ruleset = new OsuRuleset().RulesetInfo },
                };

                beatmap.ControlPointInfo.Add(0, new TimingControlPoint());

                editorBeatmap = new EditorBeatmap(beatmap, new LegacyBeatmapSkin(beatmap.BeatmapInfo, null));

                Beatmap.Value = CreateWorkingBeatmap(editorBeatmap.PlayableBeatmap);

                Child = new PopoverContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = new DependencyProvidingContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        CachedDependencies = new (Type, object)[]
                        {
                            (typeof(EditorBeatmap), editorBeatmap),
                            (typeof(IBeatSnapProvider), editorBeatmap),
                            (typeof(OverlayColourProvider), new OverlayColourProvider(OverlayColourScheme.Green)),
                        },
                        Children = new Drawable[]
                        {
                            editorBeatmap,
                            new ComposeScreen { State = { Value = Visibility.Visible } },
                        }
                    }
                };
            });

            AddUntilStep("wait for composer", () => this.ChildrenOfType<HitObjectComposer>().SingleOrDefault()?.IsLoaded == true);
            AddUntilStep("wait for difficulty display", () =>
                this.ChildrenOfType<OsuDifficultyRatingBadge>().SingleOrDefault()?.IsLoaded == true);
            AddUntilStep("wait for placement timeline", () => this.ChildrenOfType<TimelineArea>().SingleOrDefault()?.IsLoaded == true);
            AddAssert("difficulty curve is not on placement timeline", () =>
                !this.ChildrenOfType<TimelineArea>().Single().ChildrenOfType<DifficultyRatingTimelineOverlay>().Any());
            AddAssert("difficulty is not calculated automatically", () =>
                !this.ChildrenOfType<OsuHitObjectComposer>().Single().StarRating.Value.HasValue);
            AddStep("manually request difficulty calculation", () => this.ChildrenOfType<OsuHitObjectComposer>().Single().UpdateDifficulty());
            AddUntilStep("wait for manually requested difficulty calculation", () =>
                this.ChildrenOfType<OsuHitObjectComposer>().SingleOrDefault()?.StarRating.Value.HasValue == true);
        }

        /// <summary>
        /// Ensures that the skin of the edited beatmap is properly wrapped in a <see cref="LegacySkinTransformer"/>.
        /// </summary>
        [Test]
        public void TestLegacyBeatmapSkinHasTransformer()
        {
            AddAssert("legacy beatmap skin has transformer", () =>
            {
                var sources = this.ChildrenOfType<BeatmapSkinProvidingContainer>().First().AllSources;
                return sources.OfType<LegacySkinTransformer>().Count(t => t.Skin == editorBeatmap.BeatmapSkin.AsNonNull().Skin) == 1;
            });
        }

        [Test]
        public void TestDifficultyDisplayPopoverLoads()
        {
            AddStep("open difficulty display panel", () =>
                this.ChildrenOfType<EditorToolButton>().Single(button => button.Text.ToString() == "Difficulty display").Selected.Value = true);
            AddUntilStep("wait for difficulty display panel", () => this.ChildrenOfType<DifficultyVisualizationPopover>().SingleOrDefault()?.IsLoaded == true);
            AddAssert("manual update button is shown", () => this.ChildrenOfType<FormButton>().Any(button => button.Caption.ToString() == "Update difficulty now"));
        }
    }
}
