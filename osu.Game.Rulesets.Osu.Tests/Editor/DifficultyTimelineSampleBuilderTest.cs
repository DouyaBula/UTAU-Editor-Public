// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Osu.Edit;

namespace osu.Game.Rulesets.Osu.Tests.Editor
{
    [TestFixture]
    public class DifficultyTimelineSampleBuilderTest
    {
        [Test]
        public void ResamplesLocalPeaksBeforeCombiningSkillContributions()
        {
            var intervals = new[]
            {
                new DifficultyTimelineInterval(0, "Aim", 0, 100, 100, 9),
                new DifficultyTimelineInterval(0, "Aim", 100, 200, 10, 9),
                new DifficultyTimelineInterval(1, "Speed", 100, 200, 100, 1),
            };

            var samples = DifficultyTimelineSampleBuilder.Build(intervals, 200, 100);

            Assert.That(samples.Count, Is.EqualTo(3));
            Assert.That(samples[0].RelativeDifficulty, Is.EqualTo(0.9).Within(0.0001));
            Assert.That(samples[1].RelativeDifficulty, Is.EqualTo(0.19).Within(0.0001));
            Assert.That(samples[2].RelativeDifficulty, Is.EqualTo(0).Within(0.0001));
            Assert.That(samples[0].SkillName, Is.EqualTo("Aim"));
            Assert.That(samples[1].SkillName, Is.EqualTo("Speed"));
        }

        [Test]
        public void ShortIntervalsAreVisibleInEveryOverlappingTimeBucket()
        {
            var intervals = new[]
            {
                new DifficultyTimelineInterval(0, "Aim", 350, 450, 100, 1),
            };

            var samples = DifficultyTimelineSampleBuilder.Build(intervals, 800, 400);

            Assert.That(samples.Select(sample => sample.RelativeDifficulty), Is.EqualTo(new[] { 1d, 1d, 0d }));
        }

        [Test]
        public void LongMapsWidenTheBucketGridWithoutDroppingTheEnd()
        {
            var intervals = new[]
            {
                new DifficultyTimelineInterval(0, "Aim", 0, 900000, 100, 1),
            };

            var samples = DifficultyTimelineSampleBuilder.Build(intervals, 900000, 400);

            Assert.That(samples.Count, Is.LessThanOrEqualTo(1500));
            Assert.That(samples[^1].Time, Is.GreaterThanOrEqualTo(900000));
        }
    }
}
