// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Testing;
using osu.Game.Configuration;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Skinning.Default;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Osu.Tests.Editor
{
    public partial class TestSceneSliderBodyFill : EditorTestScene
    {
        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        protected override Ruleset CreateEditorRuleset() => new OsuRuleset();

        [Test]
        public void TestLiveFillToggle()
        {
            AddStep("show slider", () =>
            {
                config.SetValue(OsuSetting.EditorSliderBodyFill, true);
                EditorClock.Seek(EditorBeatmap.HitObjects.OfType<Slider>().First().StartTime);
            });
            AddUntilStep("body paths loaded", () => this.ChildrenOfType<DrawableSliderPath>().Any());
            AddStep("disable body fill", () => config.SetValue(OsuSetting.EditorSliderBodyFill, false));
            AddUntilStep("all paths reflect disabled setting", () => this.ChildrenOfType<DrawableSliderPath>().All(p => !p.RenderBody.Value));
            AddStep("enable body fill", () => config.SetValue(OsuSetting.EditorSliderBodyFill, true));
            AddUntilStep("all paths reflect enabled setting", () => this.ChildrenOfType<DrawableSliderPath>().All(p => p.RenderBody.Value));
        }
    }
}
