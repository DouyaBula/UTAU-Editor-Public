// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Edit;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Timing;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Tests.Editing
{
    [HeadlessTest]
    public partial class TestSceneSliderVelocityInheritance : EditorClockTestScene
    {
        private EditorBeatmap beatmap = null!;
        private OsuSliderVelocityToolboxGroup toolbox = null!;
        private SliderVelocityAdjustmentControl control => toolbox.ChildrenOfType<SliderVelocityAdjustmentControl>().Single();
        private Slider first = null!;
        private Slider second = null!;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create sliders and velocity toolbox", () =>
            {
                EditorClock.Stop();
                EditorClock.Seek(4000);
                beatmap = new EditorBeatmap(new OsuBeatmap { BeatmapInfo = { Ruleset = new OsuRuleset().RulesetInfo } });
                beatmap.ControlPointInfo.Add(0, new TimingControlPoint { BeatLength = 500 });
                beatmap.AddRange(new HitObject[] { first = createSlider(1000, 1.234567), second = createSlider(3000, 2.345678) });
                Child = new DependencyProvidingContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    CachedDependencies = new (Type, object)[] { (typeof(EditorBeatmap), beatmap) },
                    Child = toolbox = new OsuSliderVelocityToolboxGroup { RelativeSizeAxes = Axes.None, Width = 250 },
                };
            });
            AddUntilStep("toolbox loaded", () => toolbox.IsLoaded && control.IsLoaded);
        }

        [Test]
        public void TestAdaptiveRefreshDoesNotLatchManualOverride()
        {
            AddStep("select first", () => beatmap.SelectedHitObjects.Add(first));
            AddUntilStep("selection reflected exactly", () => control.Current.Value, () => Is.EqualTo(1.234567));
            AddStep("adaptive change and deselect in same frame", () =>
            {
                first.SliderVelocityMultiplier = 1.7654321;
                beatmap.Update(first);
                beatmap.SaveState();
                beatmap.SelectedHitObjects.Clear();
            });
            AddUntilStep("follow last slider again", () => control.Current.Value, () => Is.EqualTo(2.345678));
            AddStep("change predecessor without flushing UI", () =>
            {
                second.SliderVelocityMultiplier = 3.456789;
                Assert.That(toolbox.GetPlacementVelocity(4000), Is.EqualTo(3.456789));
                Assert.That(toolbox.GetPlacementVelocity(2000), Is.EqualTo(1.7654321));
                beatmap.Update(second);
                beatmap.SaveState();
            });
            AddUntilStep("readout follows changed predecessor", () => control.Current.Value, () => Is.EqualTo(3.456789));
            AddStep("seek before first slider", () => EditorClock.Seek(500));
            AddUntilStep("reset to default before first slider", () => control.Current.Value, () => Is.EqualTo(1));
        }

        [Test]
        public void TestExplicitOverrideAndSelectionStillWork()
        {
            AddUntilStep("last velocity shown", () => control.Current.Value, () => Is.EqualTo(2.345678));
            AddStep("manual default", () => control.Current.Value = 4);
            AddStep("seek and modify predecessor", () =>
            {
                EditorClock.Seek(2000);
                first.SliderVelocityMultiplier = 1.5;
                beatmap.Update(first);
                beatmap.SaveState();
            });
            AddAssert("manual override retained", () => toolbox.GetPlacementVelocity(2000), () => Is.EqualTo(4));
            AddStep("select resets override", () => beatmap.SelectedHitObjects.Add(first));
            AddUntilStep("selection shown", () => control.Current.Value, () => Is.EqualTo(1.5));
            AddStep("edit selected velocity", () => control.Current.Value = 2.5);
            AddAssert("object updated", () => first.SliderVelocityMultiplier, () => Is.EqualTo(2.5));
            AddStep("deselect and seek", () =>
            {
                beatmap.SelectedHitObjects.Clear();
                EditorClock.Seek(4000);
            });
            AddUntilStep("automatic inheritance restored", () => control.Current.Value, () => Is.EqualTo(2.345678));
        }

        [Test]
        public void TestProgrammaticRefreshDoesNotApplyRoundedVelocity()
        {
            int edits = 0;
            AddStep("observe explicit edits", () => control.VelocityEdited += () => edits++);
            AddStep("select slider", () => beatmap.SelectedHitObjects.Add(first));
            AddUntilStep("selection reflected", () => control.Current.Value, () => Is.EqualTo(1.234567));
            AddStep("refresh from model", () =>
            {
                first.SliderVelocityMultiplier = 1.987654321;
                beatmap.Update(first);
                beatmap.SaveState();
            });
            AddUntilStep("exact velocity retained", () => control.Current.Value, () => Is.EqualTo(1.987654321));
            AddAssert("no user edits emitted", () => edits, () => Is.Zero);
            AddAssert("object not rounded", () => first.SliderVelocityMultiplier, () => Is.EqualTo(1.987654321));
        }

        private static Slider createSlider(double time, double velocity) => new Slider
        {
            StartTime = time,
            SliderVelocityMultiplier = velocity,
            Path = new SliderPath(PathType.LINEAR, new[] { Vector2.Zero, new Vector2(100, 0) }),
        };
    }
}
