// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Reflection;
using NUnit.Framework;
using osu.Game.Rulesets.Osu.Skinning;
using osu.Game.Rulesets.Osu.Skinning.Argon;
using osu.Game.Rulesets.Osu.Skinning.Default;
using osu.Game.Rulesets.Osu.Skinning.Legacy;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Tests.Editor
{
    [TestFixture]
    public class SliderBodyFillTest
    {
        [TestCase(typeof(LegacySliderBody))]
        [TestCase(typeof(ArgonSliderBody))]
        [TestCase(typeof(ManualSliderBody))]
        public void TestFillTogglePreservesBorder(Type bodyType)
        {
            using var body = (SliderBody)Activator.CreateInstance(bodyType)!;
            var path = (DrawableSliderPath)typeof(SliderBody).GetProperty("Path", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(body)!;
            var colourAt = path.GetType().GetMethod("ColourAt", BindingFlags.Instance | BindingFlags.NonPublic)!;
            Color4 sample(float position) => (Color4)colourAt.Invoke(path, new object[] { position })!;

            Assert.That(path.RenderBody.Value, Is.True);
            var filled = sample(0.75f);
            var border = sample(0.1f);
            Assert.That(filled.A, Is.GreaterThan(0));
            Assert.That(border.A, Is.GreaterThan(0));
            path.RenderBody.Value = false;
            Assert.That(sample(0.75f).A, Is.Zero);
            Assert.That(sample(0.1f), Is.EqualTo(border));
            path.RenderBody.Value = true;
            Assert.That(sample(0.75f), Is.EqualTo(filled));
        }
    }
}
