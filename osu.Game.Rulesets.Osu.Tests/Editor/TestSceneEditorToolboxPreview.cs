// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Configuration;
using osu.Framework.Allocation;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Rulesets.Osu.Edit;
using osu.Game.Rulesets.Osu.Edit.DancePose;
using osu.Game.Screens.Edit.Components;
using osu.Game.Tests.Beatmaps;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Tests.Editor
{
    [HeadlessTest]
    public partial class TestSceneEditorToolboxPreview : TestSceneOsuEditor
    {
        [Resolved] private OsuConfigManager config { get; set; } = null!;
        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new TestBeatmap(ruleset, false);

        [TestCase("Slider Toolbox")]
        [TestCase("Slider Toolbox 2")]
        [TestCase("Slider Toolbox 3")]
        [TestCase("Polygon")]
        public void CancelRestoresBeatmapAndAllowsFurtherEdits(string tool)
        {
            EditorToolButton button = null!;
            EditorPreviewPopover panel = null!;
            int before = 0;
            AddStep("stop and record count", () =>
            {
                EditorClock.Stop();
                EditorClock.Seek(5000);
                before = EditorBeatmap.HitObjects.Count;
                button = Editor.ChildrenOfType<GenerateToolboxGroup>().Single().ChildrenOfType<EditorToolButton>().Single(b => b.Text.ToString() == tool);
            });
            AddStep("open tool", () => button.TriggerClick());
            AddUntilStep("preview loaded", () =>
            {
                panel = Editor.ChildrenOfType<EditorPreviewPopover>().FirstOrDefault(p => p.State.Value == Visibility.Visible)!;
                return panel?.IsLoaded == true;
            });
            AddWaitStep("settle preview", 5);
            AddStep("adjust parameters", () =>
            {
                var parameter = panel.ChildrenOfType<FormSliderBar<int>>().FirstOrDefault();
                if (parameter != null)
                    parameter.Current.Value++;
            });
            AddStep("cancel with button", () =>
            {
                var cancel = panel.ChildrenOfType<RoundedButton>().Single(b => b.Text.ToString() == "Cancel");
                InputManager.MoveMouseTo(cancel);
                InputManager.Click(MouseButton.Left);
            });
            AddWaitStep("settle cancellation", 5);
            AddAssert("cancel closed panel", () => panel.State.Value == Visibility.Hidden);
            AddAssert("no leaked objects", () => EditorBeatmap.HitObjects.Count == before);
            AddStep("edit and undo afterwards", () =>
            {
                EditorBeatmap.Add(new Objects.HitCircle { StartTime = 20000 });
                Editor.Undo();
            });
            AddAssert("undo is not blocked", () => EditorBeatmap.HitObjects.Count == before);
        }

        [TestCase("Slider Toolbox")]
        [TestCase("Slider Toolbox 2")]
        [TestCase("Polygon")]
        public void ApplyUsesLockedTimeAndCreatesOneUndoStep(string tool)
        {
            EditorPreviewPopover panel = null!;
            int before = 0;
            double lockedTime = 0;
            AddStep("configure simple generation", () =>
            {
                EditorClock.Stop();
                EditorClock.Seek(5000);
                EditorBeatmap.ControlPointInfo.Add(0, new TimingControlPoint { BeatLength = 500 });
                config.SetValue(OsuSetting.EditorParametricSliderAnimationFrames, 2f);
                config.SetValue(OsuSetting.EditorParametricSliderAnimationFrameDurationSnaps, 4f);
                before = EditorBeatmap.HitObjects.Count;
                Editor.ChildrenOfType<GenerateToolboxGroup>().Single().ChildrenOfType<EditorToolButton>().Single(b => b.Text.ToString() == tool).TriggerClick();
            });
            AddUntilStep("preview generated", () =>
            {
                panel = Editor.ChildrenOfType<EditorPreviewPopover>().FirstOrDefault(p => p.State.Value == Visibility.Visible)!;
                return panel?.IsLoaded == true && EditorBeatmap.HitObjects.Count > before;
            });
            AddStep("seek elsewhere", () =>
            {
                lockedTime = panel.ChildrenOfType<EditorToolTimeAnchor>().Single().Current.Value;
                EditorClock.Seek(12000);
            });
            AddStep("apply from fixed footer", () =>
            {
                var apply = panel.ChildrenOfType<EditorToolActionRow>().Single().ChildrenOfType<RoundedButton>().Single(b => b.Text.ToString() == "Apply");
                Assert.That(apply.Enabled.Value, Is.True);
                InputManager.MoveMouseTo(apply);
                InputManager.Click(MouseButton.Left);
            });
            AddUntilStep("panel closed", () => panel.State.Value == Visibility.Hidden);
            AddAssert("insertion stayed at locked snapped time", () => EditorBeatmap.HitObjects.Min(h => h.StartTime), () => Is.EqualTo(lockedTime));
            AddStep("undo once", () => Editor.Undo());
            AddAssert("all preview objects removed", () => EditorBeatmap.HitObjects.Count == before);
        }
    }
}
