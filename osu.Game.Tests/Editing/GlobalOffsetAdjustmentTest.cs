// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.IO;
using NUnit.Framework;
using osu.Game.Audio;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Formats;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Timing;
using osuTK;

namespace osu.Game.Tests.Editing
{
    [TestFixture]
    public class GlobalOffsetAdjustmentTest
    {
        [TestCase(1000)] // A destination collides with an old timing group's timestamp.
        [TestCase(-100)] // Negative red-line offsets must remain valid.
        public void TestWholeTimelineTranslation(int offset)
        {
            using var beatmap = createBeatmap();
            var controlPoints = beatmap.ControlPointInfo;
            var pointTimes = controlPoints.AllControlPoints.Select(p => p.Time).ToArray();
            var beatLengths = controlPoints.TimingPoints.Select(p => p.BeatLength).ToArray();
            var starts = beatmap.HitObjects.Select(h => h.StartTime).ToArray();
            var ends = beatmap.HitObjects.Select(h => h.GetEndTime()).ToArray();
            var slider = beatmap.HitObjects.OfType<Slider>().Single();
            var nestedTimes = slider.NestedHitObjects.Select(h => h.StartTime).ToArray();
            double velocity = slider.Velocity;
            double length = slider.Path.Distance;
            var breaks = beatmap.Breaks.ToArray();
            int[] bookmarks = beatmap.Bookmarks.ToArray();

            Assert.That(GlobalOffsetAdjustment.Apply(beatmap, offset, out var error), Is.True, error);

            Assert.Multiple(() =>
            {
                Assert.That(beatmap.ControlPointInfo, Is.SameAs(controlPoints));
                Assert.That(controlPoints.AllControlPoints.Select(p => p.Time), Is.EqualTo(pointTimes.Select(t => t + offset)));
                Assert.That(controlPoints.TimingPoints.Select(p => p.BeatLength), Is.EqualTo(beatLengths));
                Assert.That(beatmap.HitObjects.Select(h => h.StartTime), Is.EqualTo(starts.Select(t => t + offset)));
                Assert.That(beatmap.HitObjects.Select(h => h.GetEndTime()), Is.EqualTo(ends.Select(t => t + offset)).Within(0.0001));
                Assert.That(slider.NestedHitObjects.Select(h => h.StartTime), Is.EqualTo(nestedTimes.Select(t => t + offset)).Within(0.0001));
                Assert.That(slider.Velocity, Is.EqualTo(velocity));
                Assert.That(slider.Path.Distance, Is.EqualTo(length));
                Assert.That(slider.SliderVelocityMultiplier, Is.EqualTo(2));
                Assert.That(slider.Kiai, Is.True);
                Assert.That(slider.Samples.Single().Volume, Is.EqualTo(65));
                Assert.That(beatmap.Breaks.Select(b => b.StartTime), Is.EqualTo(breaks.Select(b => b.StartTime + offset)));
                Assert.That(beatmap.Breaks.Select(b => b.EndTime), Is.EqualTo(breaks.Select(b => b.EndTime + offset)));
                Assert.That(beatmap.Breaks.Single(), Is.TypeOf<ManualBreakPeriod>());
                Assert.That(beatmap.Bookmarks, Is.EqualTo(bookmarks.Select(t => t + offset)));
                Assert.That(beatmap.PreviewTime.Value, Is.EqualTo(3000 + offset));
            });
        }

        [Test]
        public void TestSingleUndoAndRedoRestoresEverything()
        {
            using var beatmap = createBeatmap();
            using var handler = new BeatmapEditorChangeHandler(beatmap);
            string before = handler.CurrentStateHash;
            string beforeText = encode(beatmap);
            Assert.That(GlobalOffsetAdjustment.Apply(beatmap, 1000, out _), Is.True);
            string after = handler.CurrentStateHash;
            Assert.That(after, Is.Not.EqualTo(before));

            handler.RestoreState(-1);
            Assert.That(beatmap.HitObjects.First().StartTime, Is.EqualTo(500));
            Assert.That(beatmap.ControlPointInfo.TimingPoints.First().Time, Is.Zero);
            Assert.That(beatmap.Bookmarks, Is.EqualTo(new[] { 1000, 2500 }));
            Assert.That(beatmap.PreviewTime.Value, Is.EqualTo(3000));
            Assert.That(handler.CanUndo.Value, Is.False);
            Assert.That(encode(beatmap), Is.EqualTo(beforeText));
            handler.SaveState();
            Assert.That(handler.CurrentStateHash, Is.EqualTo(before));

            handler.RestoreState(1);
            Assert.That(beatmap.HitObjects.First().StartTime, Is.EqualTo(1500));
            Assert.That(beatmap.PreviewTime.Value, Is.EqualTo(4000));
            handler.SaveState();
            Assert.That(handler.CurrentStateHash, Is.EqualTo(after));
        }

        [TestCase(-501)]
        [TestCase(int.MaxValue)]
        [TestCase(int.MinValue)]
        public void TestInvalidShiftDoesNotPartiallyMutate(int offset)
        {
            using var beatmap = createBeatmap();
            using var handler = new BeatmapEditorChangeHandler(beatmap);
            string before = handler.CurrentStateHash;
            Assert.That(GlobalOffsetAdjustment.Apply(beatmap, offset, out var error), Is.False);
            Assert.That(error, Is.Not.Null);
            handler.SaveState();
            Assert.That(handler.CurrentStateHash, Is.EqualTo(before));
            Assert.That(handler.CanUndo.Value, Is.False);
        }

        [Test]
        public void TestUnsetPreviewAndZeroOffset()
        {
            using var beatmap = createBeatmap();
            beatmap.PreviewTime.Value = -1;
            using var handler = new BeatmapEditorChangeHandler(beatmap);
            Assert.That(GlobalOffsetAdjustment.Apply(beatmap, 0, out var error), Is.False);
            Assert.That(error, Is.Null);
            Assert.That(handler.CanUndo.Value, Is.False);
            Assert.That(GlobalOffsetAdjustment.Apply(beatmap, 250, out _), Is.True);
            Assert.That(beatmap.PreviewTime.Value, Is.EqualTo(-1));
        }

        [Test]
        public void TestMarkerBeforeZeroRejectsWholeShift()
        {
            using var beatmap = createBeatmap();
            beatmap.Bookmarks.Add(0);
            Assert.That(GlobalOffsetAdjustment.Apply(beatmap, -1, out var error), Is.False);
            Assert.That(error, Does.Contain("bookmark"));
            Assert.That(beatmap.HitObjects.First().StartTime, Is.EqualTo(500));
        }

        [Test]
        public void TestTimingOnlyMapUndo()
        {
            using var beatmap = new EditorBeatmap(new OsuBeatmap { BeatmapInfo = { Ruleset = new OsuRuleset().RulesetInfo } });
            beatmap.ControlPointInfo.Add(0, new TimingControlPoint { BeatLength = 500 });
            beatmap.ControlPointInfo.Add(1000, new TimingControlPoint { BeatLength = 400 });
            using var handler = new BeatmapEditorChangeHandler(beatmap);
            Assert.That(GlobalOffsetAdjustment.Apply(beatmap, 1000, out _), Is.True);
            Assert.That(beatmap.ControlPointInfo.TimingPoints.Select(p => p.Time), Is.EqualTo(new[] { 1000, 2000 }));
            handler.RestoreState(-1);
            Assert.That(beatmap.ControlPointInfo.TimingPoints.Select(p => p.Time), Is.EqualTo(new[] { 0, 1000 }));
        }

        private static EditorBeatmap createBeatmap()
        {
            var beatmap = new EditorBeatmap(new OsuBeatmap { BeatmapInfo = { Ruleset = new OsuRuleset().RulesetInfo } });
            beatmap.ControlPointInfo.Add(0, new TimingControlPoint { BeatLength = 500 });
            beatmap.ControlPointInfo.Add(1000, new TimingControlPoint { BeatLength = 400, OmitFirstBarLine = true });
            beatmap.ControlPointInfo.Add(1000, new EffectControlPoint { KiaiMode = true });
            beatmap.ControlPointInfo.Add(2500, new TimingControlPoint { BeatLength = 600 });
            beatmap.ControlPointInfo.Add(2500, new EffectControlPoint { KiaiMode = false });
            beatmap.AddRange(new HitObject[]
            {
                new HitCircle { StartTime = 500, Samples = new[] { new HitSampleInfo(HitSampleInfo.HIT_NORMAL) } },
                new Slider
                {
                    StartTime = 1100,
                    RepeatCount = 1,
                    SliderVelocityMultiplier = 2,
                    Path = new SliderPath(PathType.BEZIER, new[] { Vector2.Zero, new Vector2(50, 80), new Vector2(100, 0) }),
                    Samples = new[] { new HitSampleInfo(HitSampleInfo.HIT_NORMAL, volume: 65) },
                },
                new Spinner { StartTime = 3500, Duration = 500, Position = new Vector2(256, 192), Samples = new[] { new HitSampleInfo(HitSampleInfo.HIT_NORMAL) } },
                new HitCircle { StartTime = 9000, Samples = new[] { new HitSampleInfo(HitSampleInfo.HIT_NORMAL) } },
            });
            beatmap.Breaks.Clear();
            beatmap.Breaks.Add(new ManualBreakPeriod(4500, 6000));
            beatmap.Bookmarks.AddRange(new[] { 1000, 2500 });
            beatmap.PreviewTime.Value = 3000;
            return beatmap;
        }

        private static string encode(EditorBeatmap beatmap)
        {
            using var writer = new StringWriter();
            new LegacyBeatmapEncoder(beatmap, beatmap.BeatmapSkin, beatmap.Storyboard).Encode(writer);
            return writer.ToString();
        }
    }
}
