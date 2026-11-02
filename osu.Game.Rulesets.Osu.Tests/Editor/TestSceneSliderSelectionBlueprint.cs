// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Edit.Blueprints.HitCircles.Components;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components;
using osu.Game.Rulesets.Osu.Edit.Curves;
using osu.Game.Rulesets.Osu.Edit;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Screens.Edit;
using osu.Game.Tests.Visual;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Tests.Editor
{
    public partial class TestSceneSliderSelectionBlueprint : SelectionBlueprintTestScene
    {
        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        private Slider slider = null!;
        private DrawableSlider drawableObject = null!;
        private TestSliderBlueprint blueprint = null!;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
            var editorBeatmap = new EditorBeatmap(new OsuBeatmap
            {
                BeatmapInfo = { Ruleset = new OsuRuleset().RulesetInfo }
            });
            dependencies.CacheAs(editorBeatmap);
            dependencies.CacheAs<IEditorChangeHandler>(new BeatmapEditorChangeHandler(editorBeatmap));
            dependencies.CacheAs(new OsuHitObjectComposer(new OsuRuleset()));
            return dependencies;
        }

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Clear();

            slider = new Slider
            {
                Position = new Vector2(256, 192),
                Path = new SliderPath(PathType.BEZIER, new[]
                {
                    Vector2.Zero,
                    new Vector2(150, 150),
                    new Vector2(300, 0)
                })
            };

            slider.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty { CircleSize = 2 });

            config.SetValue(OsuSetting.EditorAnchorMode, false);
            config.SetValue(OsuSetting.EditorClassicSymmetry, false);
            config.SetValue(OsuSetting.EditorClassicSymmetryAxis, EditorClassicSymmetryAxis.Vertical);

            Add(drawableObject = new DrawableSlider(slider));
            AddBlueprint(blueprint = new TestSliderBlueprint(slider), drawableObject);
        });

        [Test]
        public void TestInitialState()
        {
            checkPositions();
        }

        [Test]
        public void TestClassicLiveSymmetryMirrorsSelectedSliderAndFollowsEdits()
        {
            AddStep("enable classic live symmetry", () => config.SetValue(OsuSetting.EditorClassicSymmetry, true));
            AddUntilStep("mirror preview is visible", () => blueprint.ClassicSymmetryPreview.Alpha > 0);
            AddAssert("mirror starts at the source head", () => blueprint.ClassicSymmetryPreview.MirroredStart,
                () => Is.EqualTo(slider.StackedPosition));

            Vector2 before = Vector2.Zero;
            AddStep("capture mirror tail", () => before = blueprint.ClassicSymmetryPreview.MirroredEnd);
            AddStep("edit source tail", () => slider.Path.ControlPoints[^1].Position += new Vector2(0, 80));
            AddUntilStep("mirror follows source edit", () => blueprint.ClassicSymmetryPreview.MirroredEnd != before);
            AddAssert("mirror tail is reflected across the vertical axis", () => blueprint.ClassicSymmetryPreview.MirroredEnd,
                () => Is.EqualTo(slider.StackedPosition + new Vector2(-slider.Path.PositionAt(1).X, slider.Path.PositionAt(1).Y)));

            AddStep("disable live symmetry", () => config.SetValue(OsuSetting.EditorClassicSymmetry, false));
            AddAssert("mirror preview is hidden", () => blueprint.ClassicSymmetryPreview.Alpha, () => Is.EqualTo(0));
        }

        [Test]
        public void TestMoveHitObject()
        {
            moveHitObject();
            checkPositions();
        }

        [Test]
        public void TestMoveAfterApplyingDefaults()
        {
            AddStep("apply defaults", () => slider.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty { CircleSize = 2 }));
            moveHitObject();
            checkPositions();
        }

        [Test]
        public void TestDeletingControlPointKeepsDurationWhenEnabled()
        {
            double originalDuration = 0;

            AddStep("enable keep duration", () =>
            {
                config.SetValue(OsuSetting.EditorAnchorKeepDuration, true);
                originalDuration = slider.Duration;
            });
            AddStep("delete an interior control point", () =>
                blueprint.ControlPointVisualiser!.Delete(new List<PathControlPoint> { slider.Path.ControlPoints[1] }));

            AddAssert("the remaining geometry is not snapped or truncated", () => slider.Path.Distance,
                () => Is.EqualTo(slider.Path.CalculatedDistance).Within(0.001));
            AddAssert("the slider duration is preserved", () => slider.Duration,
                () => Is.EqualTo(originalDuration).Within(0.1));
            AddAssert("the slider remains a valid two-point path", () => slider.Path.ControlPoints.Count,
                () => Is.EqualTo(2));
        }

        [Test]
        public void TestAddingBezierControlPointKeepsDurationWhenEnabled()
        {
            double originalDuration = 0;

            AddStep("enable keep duration", () =>
            {
                config.SetValue(OsuSetting.EditorAnchorKeepDuration, true);
                originalDuration = slider.Duration;
            });
            AddStep("ctrl-click the Bezier path to add a control point", () =>
            {
                Vector2 position = slider.Position + slider.Path.PositionAt(0.5);
                InputManager.MoveMouseTo(drawableObject.Parent!.ToScreenSpace(position));
                InputManager.PressKey(Key.ControlLeft);
                InputManager.Click(MouseButton.Left);
                InputManager.ReleaseKey(Key.ControlLeft);
            });

            AddAssert("a control point was inserted", () => slider.Path.ControlPoints.Count, () => Is.EqualTo(4));
            AddAssert("duration is preserved after insertion", () => slider.Duration,
                () => Is.EqualTo(originalDuration).Within(0.1));
            AddAssert("Keep duration uses the inserted native geometry", () => slider.Path.Distance,
                () => Is.EqualTo(slider.Path.CalculatedDistance).Within(0.001));
        }

        [Test]
        public void TestStackedHitObject()
        {
            AddStep("set stacking", () => slider.StackHeight = 5);
            checkPositions();
        }

        [Test]
        public void TestSingleControlPointSelection()
        {
            moveMouseToControlPoint(0);
            AddStep("click", () => InputManager.Click(MouseButton.Left));
            checkControlPointSelected(0, true);
            checkControlPointSelected(1, false);
        }

        [Test]
        public void TestSingleControlPointDeselectionViaOtherControlPoint()
        {
            moveMouseToControlPoint(0);
            AddStep("click", () => InputManager.Click(MouseButton.Left));

            moveMouseToControlPoint(1);
            AddStep("click", () => InputManager.Click(MouseButton.Left));
            checkControlPointSelected(0, false);
            checkControlPointSelected(1, true);
        }

        [Test]
        public void TestSingleControlPointDeselectionViaClickOutside()
        {
            moveMouseToControlPoint(0);
            AddStep("click", () => InputManager.Click(MouseButton.Left));

            AddStep("move mouse outside control point", () => InputManager.MoveMouseTo(drawableObject));
            AddStep("click", () => InputManager.Click(MouseButton.Left));
            checkControlPointSelected(0, false);
            checkControlPointSelected(1, false);
        }

        [Test]
        public void TestMultipleControlPointSelection()
        {
            moveMouseToControlPoint(0);
            AddStep("click", () => InputManager.Click(MouseButton.Left));
            moveMouseToControlPoint(1);
            AddStep("ctrl + click", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.Click(MouseButton.Left);
                InputManager.ReleaseKey(Key.ControlLeft);
            });
            checkControlPointSelected(0, true);
            checkControlPointSelected(1, true);
        }

        [Test]
        public void TestMultipleControlPointDeselectionViaOtherControlPoint()
        {
            moveMouseToControlPoint(0);
            AddStep("click", () => InputManager.Click(MouseButton.Left));
            moveMouseToControlPoint(1);
            AddStep("ctrl + click", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.Click(MouseButton.Left);
                InputManager.ReleaseKey(Key.ControlLeft);
            });

            moveMouseToControlPoint(2);
            AddStep("click", () => InputManager.Click(MouseButton.Left));
            checkControlPointSelected(0, false);
            checkControlPointSelected(1, false);
        }

        [Test]
        public void TestMultipleControlPointDeselectionViaClickOutside()
        {
            moveMouseToControlPoint(0);
            AddStep("click", () => InputManager.Click(MouseButton.Left));
            moveMouseToControlPoint(1);
            AddStep("ctrl + click", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.Click(MouseButton.Left);
                InputManager.ReleaseKey(Key.ControlLeft);
            });

            AddStep("move mouse outside control point", () => InputManager.MoveMouseTo(drawableObject));
            AddStep("click", () => InputManager.Click(MouseButton.Left));
            checkControlPointSelected(0, false);
            checkControlPointSelected(1, false);
        }

        [Test]
        public void TestAdjustLength()
        {
            AddStep("move mouse to drag marker", () =>
            {
                Vector2 position = slider.Position + slider.Path.PositionAt(1) + new Vector2(60, 0);
                InputManager.MoveMouseTo(drawableObject.Parent!.ToScreenSpace(position));
            });
            AddStep("start drag", () => InputManager.PressButton(MouseButton.Left));
            AddStep("move mouse to control point 1", () =>
            {
                Vector2 position = slider.Position + slider.Path.ControlPoints[1].Position + new Vector2(60, 0);
                InputManager.MoveMouseTo(drawableObject.Parent!.ToScreenSpace(position));
            });
            AddStep("end adjust length", () => InputManager.ReleaseButton(MouseButton.Left));
            AddAssert("expected distance halved",
                () => Precision.AlmostEquals(slider.Path.Distance, 172.2, 0.1));

            AddStep("move mouse to drag marker", () =>
            {
                Vector2 position = slider.Position + slider.Path.PositionAt(1) + new Vector2(60, 0);
                InputManager.MoveMouseTo(drawableObject.Parent!.ToScreenSpace(position));
            });
            AddStep("start drag", () => InputManager.PressButton(MouseButton.Left));
            AddStep("move mouse beyond last control point", () =>
            {
                Vector2 position = slider.Position + slider.Path.ControlPoints[2].Position + new Vector2(100, 0);
                InputManager.MoveMouseTo(drawableObject.Parent!.ToScreenSpace(position));
            });
            AddStep("end adjust length", () => InputManager.ReleaseButton(MouseButton.Left));
            AddAssert("expected distance is calculated distance",
                () => Precision.AlmostEquals(slider.Path.Distance, slider.Path.CalculatedDistance, 0.1));

            moveMouseToControlPoint(1);
            AddAssert("expected distance is unchanged",
                () => Precision.AlmostEquals(slider.Path.Distance, slider.Path.CalculatedDistance, 0.1));
        }

        [Test]
        public void TestAnchorDoubleClickInsertsBezierAnchor()
        {
            AddStep("enable unified curve controls", () => config.SetValue(OsuSetting.EditorAnchorMode, true));
            AddUntilStep("anchor overlay is active", () => this.ChildrenOfType<AnchorPathOverlay>().Any());

            AddStep("move to curve", () =>
            {
                Vector2 position = slider.Position + slider.Path.PositionAt(0.5);
                InputManager.MoveMouseTo(drawableObject.Parent!.ToScreenSpace(position));
            });
            AddRepeatStep("double-click curve", () => InputManager.Click(MouseButton.Left), 2);

            AddAssert("curve split into two native Bezier segments", () => slider.Path.ControlPoints.Count, () => Is.EqualTo(7));
            AddAssert("anchor overlay remains active", () => this.ChildrenOfType<AnchorPathOverlay>().Any());
        }

        [Test]
        public void TestAnchorDeleteRemovesSelectedAnchor()
        {
            AddStep("create a three-anchor Bezier path", () =>
            {
                Assert.That(AnchorPath.TryRead(slider.Path, out var path), Is.True);
                Assert.That(path.Split(0, 0.5f), Is.EqualTo(1));
                path.WriteTo(slider.Path);
            });
            AddStep("enable unified curve controls", () => config.SetValue(OsuSetting.EditorAnchorMode, true));
            AddUntilStep("anchor overlay is active", () => this.ChildrenOfType<AnchorPathOverlay>().Any());

            AddStep("move to middle anchor", () =>
            {
                Assert.That(AnchorPath.TryRead(slider.Path, out var path), Is.True);
                Vector2 position = slider.Position + path.Anchors[1].Position;
                InputManager.MoveMouseTo(drawableObject.Parent!.ToScreenSpace(position));
            });
            AddStep("select middle anchor", () => InputManager.Click(MouseButton.Left));
            AddStep("delete selected anchor", () => InputManager.Key(Key.Delete));

            AddAssert("only selected anchor was deleted", () => slider.Path.ControlPoints.Count, () => Is.EqualTo(4));
            AddAssert("slider remains present", () => this.ChildrenOfType<DrawableSlider>().Any());
        }

        [Test]
        public void TestAnchorCtrlClickMultiSelects()
        {
            AddStep("create a four-anchor Bezier path", () =>
            {
                Assert.That(AnchorPath.TryRead(slider.Path, out var path), Is.True);
                Assert.That(path.Split(0, 0.33f), Is.EqualTo(1));
                Assert.That(path.Split(1, 0.5f), Is.EqualTo(2));
                path.WriteTo(slider.Path);
            });
            AddStep("enable unified curve controls", () => config.SetValue(OsuSetting.EditorAnchorMode, true));
            AddUntilStep("anchor overlay is active", () => this.ChildrenOfType<AnchorPathOverlay>().Any());

            AddStep("select first middle anchor", () =>
            {
                Assert.That(AnchorPath.TryRead(slider.Path, out var path), Is.True);
                InputManager.MoveMouseTo(drawableObject.Parent!.ToScreenSpace(slider.Position + path.Anchors[1].Position));
                InputManager.Click(MouseButton.Left);
            });
            AddStep("ctrl-select second middle anchor", () =>
            {
                Assert.That(AnchorPath.TryRead(slider.Path, out var path), Is.True);
                InputManager.MoveMouseTo(drawableObject.Parent!.ToScreenSpace(slider.Position + path.Anchors[2].Position));
                InputManager.PressKey(Key.ControlLeft);
                InputManager.Click(MouseButton.Left);
                InputManager.ReleaseKey(Key.ControlLeft);
            });
            AddStep("delete both selected anchors", () => InputManager.Key(Key.Delete));

            AddAssert("Ctrl selected both anchors", () => slider.Path.ControlPoints.Count, () => Is.EqualTo(4));
            AddAssert("slider remains present", () => this.ChildrenOfType<DrawableSlider>().Any());
        }

        [Test]
        public void TestAnchorCShortcutEditsNodeInsteadOfPlayback()
        {
            AddStep("enable unified curve controls", () => config.SetValue(OsuSetting.EditorAnchorMode, true));
            AddUntilStep("anchor overlay is active", () => this.ChildrenOfType<AnchorPathOverlay>().Any());

            Vector2 before = Vector2.Zero;
            AddStep("capture original handle", () => before = slider.Path.ControlPoints[1].Position);
            AddStep("select first anchor", () =>
            {
                Assert.That(AnchorPath.TryRead(slider.Path, out var path), Is.True);
                InputManager.MoveMouseTo(drawableObject.Parent!.ToScreenSpace(slider.Position + path.Anchors[0].Position));
                InputManager.Click(MouseButton.Left);
            });
            AddStep("press C", () => InputManager.Key(Key.C));

            AddAssert("C edited the selected node", () => slider.Path.ControlPoints[1].Position, () => Is.Not.EqualTo(before));
            AddAssert("anchor overlay remains active", () => this.ChildrenOfType<AnchorPathOverlay>().Any());
        }

        [Test]
        public void TestAnchorAltArrowNudgesSelectedAnchor()
        {
            AddStep("enable unified curve controls", () => config.SetValue(OsuSetting.EditorAnchorMode, true));
            AddUntilStep("anchor overlay is active", () => this.ChildrenOfType<AnchorPathOverlay>().Any());

            Vector2 before = Vector2.Zero;
            AddStep("select first anchor", () =>
            {
                before = slider.Position;
                Assert.That(AnchorPath.TryRead(slider.Path, out var path), Is.True);
                InputManager.MoveMouseTo(drawableObject.Parent!.ToScreenSpace(slider.Position + path.Anchors[0].Position));
                InputManager.Click(MouseButton.Left);
            });
            AddStep("alt-right nudge", () =>
            {
                InputManager.PressKey(Key.AltLeft);
                InputManager.Key(Key.Right);
                InputManager.ReleaseKey(Key.AltLeft);
            });

            AddAssert("Alt-right nudged the selected anchor", () => slider.Position.X, () => Is.EqualTo(before.X + 1).Within(0.01f));
            AddAssert("anchor overlay remains active", () => this.ChildrenOfType<AnchorPathOverlay>().Any());
        }

        [Test]
        public void TestAnchorAltPShortcutsBetweenEditingModes()
        {
            AddStep("enable unified curve controls", () => config.SetValue(OsuSetting.EditorAnchorMode, true));
            AddUntilStep("anchor overlay is active", () => this.ChildrenOfType<AnchorPathOverlay>().Any());
            AddStep("toggle to classic with Alt+P", () =>
            {
                InputManager.PressKey(Key.AltLeft);
                InputManager.Key(Key.P);
                InputManager.ReleaseKey(Key.AltLeft);
            });
            AddUntilStep("classic visualiser is active", () => this.ChildrenOfType<PathControlPointVisualiser<Slider>>().Any());
            AddStep("toggle back to anchors with Alt+P", () =>
            {
                InputManager.PressKey(Key.AltLeft);
                InputManager.Key(Key.P);
                InputManager.ReleaseKey(Key.AltLeft);
            });
            AddUntilStep("anchor overlay is active again", () => this.ChildrenOfType<AnchorPathOverlay>().Any());
        }

        [Test]
        public void TestAnchorShiftRightClickRetractsOnlyHandle()
        {
            AddStep("enable unified curve controls", () => config.SetValue(OsuSetting.EditorAnchorMode, true));
            AddUntilStep("anchor overlay is active", () => this.ChildrenOfType<AnchorPathOverlay>().Any());

            Vector2 outgoing = Vector2.Zero;
            AddStep("select first anchor", () =>
            {
                Assert.That(AnchorPath.TryRead(slider.Path, out var path), Is.True);
                outgoing = path.Anchors[0].Outgoing;
                Assert.That(outgoing.LengthSquared, Is.GreaterThan(0));
                InputManager.MoveMouseTo(drawableObject.Parent!.ToScreenSpace(slider.Position + path.Anchors[0].Position));
                InputManager.Click(MouseButton.Left);
            });
            AddStep("shift right-click outgoing handle", () =>
            {
                Assert.That(AnchorPath.TryRead(slider.Path, out var path), Is.True);
                InputManager.MoveMouseTo(drawableObject.Parent!.ToScreenSpace(slider.Position + path.Anchors[0].Position + path.Anchors[0].Outgoing));
                InputManager.PressKey(Key.ShiftLeft);
                InputManager.PressButton(MouseButton.Right);
                InputManager.ReleaseButton(MouseButton.Right);
                InputManager.ReleaseKey(Key.ShiftLeft);
            });

            AddAssert("only the handle was retracted", () =>
            {
                Assert.That(AnchorPath.TryRead(slider.Path, out var path), Is.True);
                return path.Anchors.Count == 2
                       && path.Anchors[0].Outgoing.LengthSquared < 0.001f
                       && path.Anchors[0].Position == Vector2.Zero;
            });
        }

        private void moveHitObject()
        {
            AddStep("move hitobject", () =>
            {
                slider.Position = new Vector2(300, 225);
            });
        }

        private void checkPositions()
        {
            AddAssert("body positioned correctly", () => blueprint.BodyPiece.Position == slider.StackedPosition);

            AddAssert("head positioned correctly",
                () => Precision.AlmostEquals(blueprint.HeadOverlay.CirclePiece.ScreenSpaceDrawQuad.Centre, drawableObject.HeadCircle.ScreenSpaceDrawQuad.Centre));

            AddAssert("tail positioned correctly",
                () => Precision.AlmostEquals(blueprint.TailOverlay.CirclePiece.ScreenSpaceDrawQuad.Centre, drawableObject.TailCircle.ScreenSpaceDrawQuad.Centre));

            AddAssert("end drag marker positioned correctly",
                () => Precision.AlmostEquals(blueprint.TailOverlay.EndDragMarker!.ToScreenSpace(blueprint.TailOverlay.EndDragMarker.OriginPosition), drawableObject.TailCircle.ScreenSpaceDrawQuad.Centre, 2));
        }

        private void moveMouseToControlPoint(int index)
        {
            AddStep($"move mouse to control point {index}", () =>
            {
                Vector2 position = slider.Position + slider.Path.ControlPoints[index].Position;
                InputManager.MoveMouseTo(drawableObject.Parent!.ToScreenSpace(position));
            });
        }

        private void checkControlPointSelected(int index, bool selected)
            => AddAssert($"control point {index} {(selected ? "selected" : "not selected")}", () => blueprint.ControlPointVisualiser!.Pieces[index].IsSelected.Value == selected);

        private partial class TestSliderBlueprint : SliderSelectionBlueprint
        {
            public new SliderBodyPiece BodyPiece => base.BodyPiece;
            public new TestSliderCircleOverlay HeadOverlay => (TestSliderCircleOverlay)base.HeadOverlay;
            public new TestSliderCircleOverlay TailOverlay => (TestSliderCircleOverlay)base.TailOverlay;
            public new PathControlPointVisualiser<Slider>? ControlPointVisualiser => base.ControlPointVisualiser;

            public TestSliderBlueprint(Slider slider)
                : base(slider)
            {
            }

            protected override SliderCircleOverlay CreateCircleOverlay(Slider slider, SliderPosition position) => new TestSliderCircleOverlay(slider, position);
        }

        private partial class TestSliderCircleOverlay : SliderCircleOverlay
        {
            public new HitCirclePiece CirclePiece => base.CirclePiece;

            public TestSliderCircleOverlay(Slider slider, SliderPosition position)
                : base(slider, position)
            {
            }
        }
    }
}
