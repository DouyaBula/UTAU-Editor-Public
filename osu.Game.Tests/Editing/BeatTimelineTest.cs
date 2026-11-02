// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Timing;

namespace osu.Game.Tests.Editing
{
    [TestFixture]
    public class BeatTimelineTest
    {
        [TestCase(-1000, 6)]
        [TestCase(0, 3)]
        [TestCase(1000, 4)]
        [TestCase(1250, 2.25)]
        [TestCase(2000, 0)]
        public void BeatArithmeticRoundTripsAcrossTimingChanges(double start, double beats)
        {
            var timing = new ControlPointInfo();
            timing.Add(0, new TimingControlPoint { BeatLength = 500 });
            timing.Add(1000, new TimingControlPoint { BeatLength = 250 });
            timing.Add(1750, new TimingControlPoint { BeatLength = 600 });
            double end = BeatTimeline.TimeAfterBeats(timing, start, beats);
            Assert.That(BeatTimeline.BeatsBetween(timing, start, end), Is.EqualTo(beats).Within(1e-8));
            Assert.That(BeatTimeline.BeatsBetween(timing, end, start), Is.EqualTo(-beats).Within(1e-8));
        }

        [Test]
        public void DurationAndGapUseTheirOwnTimingSegments()
        {
            var timing = new ControlPointInfo();
            timing.Add(0, new TimingControlPoint { BeatLength = 500 });
            timing.Add(1000, new TimingControlPoint { BeatLength = 250 });
            double end = BeatTimeline.TimeAfterBeats(timing, 750, 1);
            Assert.That(end, Is.EqualTo(1125));
            Assert.That(BeatTimeline.TimeAfterBeats(timing, end, 0.25), Is.EqualTo(1187.5));
        }

        [Test]
        public void NonFiniteAndNegativeBeatCountsAreRejected()
        {
            var timing = new ControlPointInfo();
            Assert.Throws<ArgumentOutOfRangeException>(() => BeatTimeline.TimeAfterBeats(timing, 0, double.NaN));
            Assert.Throws<ArgumentOutOfRangeException>(() => BeatTimeline.TimeAfterBeats(timing, 0, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => BeatTimeline.BeatsBetween(timing, 0, double.PositiveInfinity));
        }
    }
}
