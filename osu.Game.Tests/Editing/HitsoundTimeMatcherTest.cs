// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Game.Screens.Edit;

namespace osu.Game.Tests.Editing
{
    [TestFixture]
    public class HitsoundTimeMatcherTest
    {
        [Test]
        public void StableTiesInclusiveBoundariesAndEligibility()
        {
            var targets = new[] { 90d, 90d, 110d };
            Assert.That(HitsoundTimeMatcher.FindNearest(targets, 100, 10, t => t), Is.EqualTo(0));
            Assert.That(HitsoundTimeMatcher.FindNearest(targets, 100, 10, t => t, i => i > 0), Is.EqualTo(1));
            Assert.That(HitsoundTimeMatcher.FindNearest(targets, 100, 9, t => t), Is.EqualTo(-1));
            Assert.That(HitsoundTimeMatcher.FindNearest(Array.Empty<double>(), 100, 10, t => t), Is.EqualTo(-1));
        }

        [Test]
        public void PreferredTypeWinsOnlyWithinTolerance()
        {
            var targets = new[] { 90d, 99d, 120d };
            Assert.That(HitsoundTimeMatcher.FindNearest(targets, 100, 10, t => t, preferred: t => t != 99), Is.EqualTo(0));
            Assert.That(HitsoundTimeMatcher.FindNearest(targets, 100, 5, t => t, preferred: t => t != 99), Is.EqualTo(1));
        }

        [Test]
        public void LargeMapsOnlyVisitNearbyTargets()
        {
            var targets = Enumerable.Range(0, 100000).Select(i => i * 100d).ToArray();
            int visited = 0;
            int match = HitsoundTimeMatcher.FindNearest(targets, 5000000, 10, t => { visited++; return t; });
            Assert.That(match, Is.EqualTo(50000));
            Assert.That(visited, Is.LessThan(25));
        }

        [TestCase(double.NaN)]
        [TestCase(double.PositiveInfinity)]
        [TestCase(-1)]
        public void InvalidToleranceIsRejected(double value)
            => Assert.Throws<ArgumentOutOfRangeException>(() => HitsoundTimeMatcher.ValidateTolerance(value));
    }
}
