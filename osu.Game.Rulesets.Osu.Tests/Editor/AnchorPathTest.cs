// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Edit.Curves;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests.Editor
{
    [TestFixture]
    public class AnchorPathTest
    {
        private static AnchorPath curve()
        {
            var path = new AnchorPath();
            path.Anchors.Add(new CurveAnchor(Vector2.Zero) { Outgoing = new Vector2(30, 90) });
            path.Anchors.Add(new CurveAnchor(new Vector2(140, 20)) { Incoming = new Vector2(-40, 60) });
            return path;
        }

        private static void close(Vector2 actual, Vector2 expected)
            => Assert.That(Vector2.Distance(actual, expected), Is.LessThan(0.001f));

        private static (EditorBeatmap beatmap, Slider slider) editableSlider()
        {
            var beatmap = new EditorBeatmap(new OsuBeatmap
            {
                BeatmapInfo = { Ruleset = new OsuRuleset().RulesetInfo }
            });
            var slider = new Slider
            {
                Position = new Vector2(100, 100), StartTime = 1000, RepeatCount = 1,
                Path = new SliderPath(curve().ToControlPoints().ToArray())
            };
            slider.ApplyDefaults(beatmap.ControlPointInfo, beatmap.Difficulty);
            beatmap.Add(slider);
            return (beatmap, slider);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestEditingTimingPolicy(bool preserveDuration)
        {
            var (beatmap, slider) = editableSlider();
            double duration = slider.Duration;
            double velocity = slider.SliderVelocityMultiplier;
            var candidate = curve();
            foreach (var a in candidate.Anchors)
            {
                a.Position *= 2; a.Incoming *= 2; a.Outgoing *= 2;
            }
            Assert.That(AnchorEditSession.TryApply(slider, candidate, beatmap, preserveDuration, out var error), Is.True, error);
            Assert.That(slider.Duration, Is.EqualTo(preserveDuration ? duration : duration * 2).Within(0.2));
            Assert.That(slider.SliderVelocityMultiplier, Is.EqualTo(preserveDuration ? velocity * 2 : velocity).Within(0.002));
            Assert.That(slider.StartTime, Is.EqualTo(1000));
            Assert.That(slider.RepeatCount, Is.EqualTo(1));
        }

        [Test]
        public void TestContinuousDragPreservesDurationAndSourceGeometry()
        {
            var (beatmap, slider) = editableSlider();
            double duration = slider.Duration;
            var originalPoints = slider.Path.ControlPoints.ToArray();

            for (int frame = 0; frame < 120; frame++)
            {
                var candidate = curve();
                candidate.Anchors[1].Position += new Vector2(frame * 0.25f, 0);
                var expectedEnd = candidate.Anchors[1].Position;
                Assert.That(AnchorEditSession.TryApply(slider, candidate, beatmap, true, out var error,
                                                       rebuildDefaults: false, notifyBeatmap: false, remember: false), Is.True, error);
                Assert.That(slider.Path.Distance, Is.EqualTo(slider.Path.CalculatedDistance).Within(0.001));
                Assert.That(slider.Duration, Is.EqualTo(duration).Within(0.01));
                close(candidate.Anchors[1].Position, expectedEnd);
                Assert.That(slider.Path.ControlPoints.Select((point, index) => ReferenceEquals(point, originalPoints[index])).All(v => v), Is.True);
            }
        }

        [Test]
        public void TestSessionUsesNewestConstraintsAndReturnsIndependentCopy()
        {
            var (_, slider) = editableSlider();
            var candidate = curve();
            AnchorEditSession.Remember(slider, candidate);
            candidate.Anchors[0].Kind = AnchorKind.Symmetric;
            AnchorEditSession.Remember(slider, candidate);
            Assert.That(AnchorEditSession.TryRead(slider, out var read), Is.True);
            Assert.That(read.Anchors[0].Kind, Is.EqualTo(AnchorKind.Symmetric));
            read.Anchors[0].Position = new Vector2(999);
            Assert.That(AnchorEditSession.TryRead(slider, out var reread), Is.True);
            close(reread.Anchors[0].Position, Vector2.Zero);
        }

        [Test]
        public void TestRejectedEditIsAtomic()
        {
            var (beatmap, slider) = editableSlider();
            var original = slider.Path.ControlPoints.Select(p => new PathControlPoint(p.Position, p.Type)).ToArray();
            var position = slider.Position;
            double duration = slider.Duration;
            var candidate = curve();
            foreach (var a in candidate.Anchors)
            {
                a.Position = a.Position * 100 + new Vector2(20); a.Incoming *= 100; a.Outgoing *= 100;
            }
            Assert.That(AnchorEditSession.TryApply(slider, candidate, beatmap, true, out var error), Is.False);
            Assert.That(error, Does.Contain("SV limits"));
            Assert.That(slider.Position, Is.EqualTo(position));
            Assert.That(slider.Path.ControlPoints.SequenceEqual(original), Is.True);
            Assert.That(slider.Duration, Is.EqualTo(duration));
        }

        [Test]
        public void TestHeadMoveUndoRedoAndModeRoundTrip()
        {
            var (beatmap, slider) = editableSlider();
            var handler = new BeatmapEditorChangeHandler(beatmap);
            var position = slider.Position;
            var original = slider.Path.ControlPoints.Select(p => new PathControlPoint(p.Position, p.Type)).ToArray();
            var candidate = curve();
            candidate.Anchors[0].Position += new Vector2(15, 10);
            handler.BeginChange();
            Assert.That(AnchorEditSession.TryApply(slider, candidate, beatmap, true, out var error), Is.True, error);
            handler.EndChange();
            close(slider.Position, position + new Vector2(15, 10));
            close(slider.Path.ControlPoints[0].Position, Vector2.Zero);
            string changedHash = handler.CurrentStateHash;
            for (int i = 0; i < 10; i++) Assert.That(AnchorEditSession.TryRead(slider, out _), Is.True);
            handler.SaveState();
            Assert.That(handler.CurrentStateHash, Is.EqualTo(changedHash));
            handler.RestoreState(-1);
            var restored = beatmap.HitObjects.OfType<Slider>().Single();
            close(restored.Position, position);
            Assert.That(restored.Path.ControlPoints.SequenceEqual(original), Is.True);
            handler.RestoreState(1);
            close(beatmap.HitObjects.OfType<Slider>().Single().Position, position + new Vector2(15, 10));
            Assert.That(handler.CurrentStateHash, Is.EqualTo(changedHash));
        }

        [Test]
        public void TestApplyKeepsTopologyAtomicAndReusesControlPoints()
        {
            var (beatmap, slider) = editableSlider();
            var originalPoints = slider.Path.ControlPoints.ToArray();
            var candidate = curve();
            candidate.Anchors[1].Outgoing = new Vector2(45, -25);

            Assert.That(AnchorEditSession.TryApply(slider, candidate, beatmap, false, out var error,
                                                   rebuildDefaults: false, notifyBeatmap: false, remember: false), Is.True, error);
            Assert.That(slider.Path.ControlPoints.Select((point, index) => ReferenceEquals(point, originalPoints[index])).All(v => v), Is.True);

            var collectionCounts = new System.Collections.Generic.List<int>();
            slider.Path.ControlPoints.CollectionChanged += (_, _) => collectionCounts.Add(slider.Path.ControlPoints.Count);
            candidate.Split(0, 0.5f);

            Assert.That(AnchorEditSession.TryApply(slider, candidate, beatmap, false, out error,
                                                   rebuildDefaults: false, notifyBeatmap: false, remember: false), Is.True, error);
            Assert.That(collectionCounts, Does.Not.Contain(0), "Anchor editing must never expose a transient empty path to the selection blueprint.");
            Assert.That(AnchorPath.TryRead(slider.Path, out var read), Is.True);
            Assert.That(read.Anchors.Count, Is.EqualTo(candidate.Anchors.Count));
        }

        [Test]
        public void TestNativeRoundTripDoesNotMutateSource()
        {
            var original = curve();
            var native = new SliderPath(original.ToControlPoints().ToArray(), 80);
            var points = native.ControlPoints.Select(p => new PathControlPoint(p.Position, p.Type)).ToArray();
            int version = native.Version.Value;
            Assert.That(AnchorPath.TryRead(native, out var result), Is.True);
            for (int i = 0; i <= 100; i++) close(result.Evaluate(0, i / 100f), original.Evaluate(0, i / 100f));
            Assert.That(native.ControlPoints.SequenceEqual(points), Is.True);
            Assert.That(native.ExpectedDistance.Value, Is.EqualTo(80));
            Assert.That(native.Version.Value, Is.EqualTo(version));
        }

        [Test]
        public void TestPiecewiseBezierRoundTripKeepsNativeStructure()
        {
            var original = new AnchorPath();
            original.Anchors.Add(new CurveAnchor(new Vector2(0, 0)) { Outgoing = new Vector2(25, 40) });
            original.Anchors.Add(new CurveAnchor(new Vector2(100, 25))
            {
                Incoming = new Vector2(-30, 35),
                Outgoing = new Vector2(35, -20)
            });
            original.Anchors.Add(new CurveAnchor(new Vector2(190, 0)) { Incoming = new Vector2(-20, -35) });

            var points = original.ToControlPoints();
            Assert.That(points.Count, Is.EqualTo(7));
            Assert.That(points.Select(p => p.Type).Where(t => t != null).Count(), Is.EqualTo(2));
            Assert.That(points[0].Type, Is.EqualTo(PathType.BEZIER));
            Assert.That(points[3].Type, Is.EqualTo(PathType.BEZIER));
            Assert.That(AnchorPath.TryRead(new SliderPath(points.ToArray()), out var read), Is.True);
            Assert.That(read.Anchors.Count, Is.EqualTo(original.Anchors.Count));

            for (int segment = 0; segment < original.SegmentCount; segment++)
            {
                for (int i = 0; i <= 100; i++)
                    close(read.Evaluate(segment, i / 100f), original.Evaluate(segment, i / 100f));
            }
        }

        [TestCase(0.1f)]
        [TestCase(0.5f)]
        [TestCase(0.9f)]
        public void TestSubdivisionIsExact(float split)
        {
            var original = curve();
            var divided = original.Clone();
            Assert.That(divided.Split(0, split), Is.EqualTo(1));
            for (int i = 0; i <= 100; i++)
            {
                float t = i / 100f;
                close(t <= split ? divided.Evaluate(0, t / split) : divided.Evaluate(1, (t - split) / (1 - split)), original.Evaluate(0, t));
            }
        }

        [Test]
        public void TestQuadraticElevation()
        {
            var control = new Vector2(40, 80);
            var end = new Vector2(100, 0);
            var native = new SliderPath(new[] { new PathControlPoint(Vector2.Zero, PathType.BEZIER), new PathControlPoint(control), new PathControlPoint(end) });
            Assert.That(AnchorPath.TryRead(native, out var path), Is.True);
            for (int i = 0; i <= 100; i++)
            {
                float t = i / 100f;
                close(path.Evaluate(0, t), 2 * (1 - t) * t * control + t * t * end);
            }
        }

        [Test]
        public void TestNonBezierSliderStaysInClassicMode()
        {
            var native = new SliderPath(new[] { new PathControlPoint(Vector2.Zero, PathType.PERFECT_CURVE), new PathControlPoint(new Vector2(50, 50)), new PathControlPoint(new Vector2(100, 0)) });
            Assert.That(AnchorPath.TryRead(native, out _), Is.False);
            Assert.That(native.ControlPoints[0].Type, Is.EqualTo(PathType.PERFECT_CURVE));
        }

        [Test]
        public void TestHandleConstraints()
        {
            var path = curve();
            path.SetKind(0, AnchorKind.Symmetric);
            path.MoveHandle(0, true, new Vector2(20, 30), false);
            close(path.Anchors[0].Incoming, new Vector2(-20, -30));
            path.SetKind(0, AnchorKind.Smooth);
            float length = path.Anchors[0].Incoming.Length;
            path.MoveHandle(0, true, new Vector2(80, 0), false);
            close(path.Anchors[0].Incoming, new Vector2(-length, 0));
            path.MoveHandle(0, true, new Vector2(0, 50), true);
            close(path.Anchors[0].Incoming, new Vector2(-length, 0));
            Assert.That(path.Anchors[0].Kind, Is.EqualTo(AnchorKind.Corner));
        }

        [Test]
        public void TestReverseAndMirrorAreReversible()
        {
            var original = curve();
            var changed = original.Clone();
            changed.Reverse();
            for (int i = 0; i <= 100; i++) close(changed.Evaluate(0, i / 100f), original.Evaluate(0, 1 - i / 100f));
            changed.Reverse();
            changed.Mirror(CurveMirror.Horizontal, 70);
            changed.Mirror(CurveMirror.Horizontal, 70);
            for (int i = 0; i <= 100; i++) close(changed.Evaluate(0, i / 100f), original.Evaluate(0, i / 100f));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestCornerTrimKeepsOnlyInnerHandles(bool bevel)
        {
            var original = curve();
            original.Anchors.Add(new CurveAnchor(new Vector2(240, 100)) { Incoming = new Vector2(-20, 20) });
            original.Anchors[1].Outgoing = new Vector2(30, -20);
            var trimmed = original.Clone();
            Assert.That(trimmed.RoundCorner(1, 0.2f, bevel), Is.True);

            // The two generated corner anchors face each other across the new
            // round/bevel span. Only those inner-facing handles may remain.
            Assert.That(trimmed.Anchors[1].Incoming, Is.EqualTo(Vector2.Zero));
            Assert.That(trimmed.Anchors[2].Outgoing, Is.EqualTo(Vector2.Zero));

            if (bevel)
            {
                Assert.That(trimmed.Anchors[1].Outgoing, Is.EqualTo(Vector2.Zero));
                Assert.That(trimmed.Anchors[2].Incoming, Is.EqualTo(Vector2.Zero));
            }
            else
            {
                Assert.That(trimmed.Anchors[1].Outgoing.LengthSquared, Is.GreaterThan(0.001f));
                Assert.That(trimmed.Anchors[2].Incoming.LengthSquared, Is.GreaterThan(0.001f));
            }
        }

        [Test]
        public void TestClassicBezierRoundAndHandleOperations()
        {
            var path = new AnchorPath();
            path.Anchors.Add(new CurveAnchor(Vector2.Zero) { Outgoing = new Vector2(30, 0) });
            path.Anchors.Add(new CurveAnchor(new Vector2(100, 80)) { Incoming = new Vector2(-20, -20), Outgoing = new Vector2(20, 20) });
            path.Anchors.Add(new CurveAnchor(new Vector2(200, 0)) { Incoming = new Vector2(-30, 0) });
            var slider = new Slider { Path = new SliderPath(path.ToControlPoints().ToArray()) };
            var corner = slider.Path.ControlPoints[3];

            // Deliberately introduce a tangent discontinuity so smoothing has observable work.
            slider.Path.ControlPoints[2].Position += new Vector2(12, 8);
            Assert.That(ClassicSliderCurveOperations.TrySmooth(slider, new[] { corner }, true), Is.True);
            Assert.That(ClassicSliderCurveOperations.TryRoundOrBevel(slider, new[] { corner }, 0.2f, false, false, out var error), Is.True, error);
            Assert.That(AnchorPath.TryRead(slider.Path, out var rounded), Is.True);
            Assert.That(rounded.Anchors.Count, Is.EqualTo(4));
            Assert.That(rounded.Anchors[1].Incoming, Is.EqualTo(Vector2.Zero));
            Assert.That(rounded.Anchors[2].Outgoing, Is.EqualTo(Vector2.Zero));
        }

        [Test]
        public void TestClassicLinearBevelAndDistribution()
        {
            var slider = new Slider
            {
                Path = new SliderPath(new[]
                {
                    new PathControlPoint(Vector2.Zero, PathType.LINEAR),
                    new PathControlPoint(new Vector2(100, 80), PathType.LINEAR),
                    new PathControlPoint(new Vector2(220, 0), PathType.LINEAR),
                })
            };
            var corner = slider.Path.ControlPoints[1];

            Assert.That(ClassicSliderCurveOperations.TryRoundOrBevel(slider, new[] { corner }, 0.2f, true, false, out var error), Is.True, error);
            Assert.That(slider.Path.ControlPoints.Count, Is.EqualTo(4));
            Assert.That(ClassicSliderCurveOperations.TryDistribute(slider,
                slider.Path.ControlPoints.Where(p => p.Type.HasValue).ToArray(), true), Is.True);
        }

        [Test]
        public void TestClassicMixedPathOperationsAcceptInheritedPoints()
        {
            // Linear -> perfect-curve -> linear is the common mixed representation emitted by
            // osu!. The perfect-curve's middle point and the final inherited point deliberately have
            // no Type; classic tools must still be able to operate on them.
            var slider = new Slider
            {
                Path = new SliderPath(new[]
                {
                    new PathControlPoint(Vector2.Zero, PathType.LINEAR),
                    new PathControlPoint(new Vector2(40, 50)),
                    new PathControlPoint(new Vector2(100, 0), PathType.PERFECT_CURVE),
                    new PathControlPoint(new Vector2(140, 60)),
                    new PathControlPoint(new Vector2(180, 0), PathType.LINEAR),
                    new PathControlPoint(new Vector2(240, 40)),
                })
            };

            var inherited = slider.Path.ControlPoints[3];
            Assert.That(ClassicSliderCurveOperations.TryAlign(slider,
                new[] { inherited, slider.Path.ControlPoints[5] }, true), Is.False);
            Assert.That(ClassicSliderCurveOperations.TrySmooth(slider, new[] { inherited }, true), Is.False);
            Assert.That(ClassicSliderCurveOperations.TryRetractHandles(slider, new[] { inherited }), Is.False);

            // Select the typed boundary between perfect-curve and linear. The operation must keep
            // the neighbouring native types while inserting a local bevel.
            var boundary = slider.Path.ControlPoints[4];
            Assert.That(ClassicSliderCurveOperations.TryRoundOrBevel(slider, new[] { boundary }, 0.2f, true, false, out var error), Is.True, error);
            Assert.That(slider.Path.ControlPoints.Count, Is.GreaterThan(6));
            Assert.That(slider.Path.ControlPoints.Any(p => p.Type?.Type == SplineType.PerfectCurve), Is.True);
            Assert.That(slider.Path.Distance, Is.GreaterThan(0));
        }

        [Test]
        public void TestClassicSmoothUsesPerfectTangentAndInsertsMissingHandle()
        {
            var slider = new Slider
            {
                Path = new SliderPath(new[]
                {
                    new PathControlPoint(Vector2.Zero, PathType.BEZIER),
                    new PathControlPoint(new Vector2(70, 80)),
                    new PathControlPoint(new Vector2(120, 0), PathType.PERFECT_CURVE),
                    new PathControlPoint(new Vector2(160, -70)),
                    new PathControlPoint(new Vector2(220, 0), PathType.LINEAR),
                })
            };

            var boundary = slider.Path.ControlPoints[2];
            Vector2 perfectMiddle = slider.Path.ControlPoints[3].Position;
            Assert.That(ClassicSliderCurveOperations.TrySmooth(slider, new[] { boundary }, true), Is.True);

            // The perfect-curve definition point is geometric data, not a free Bézier handle.
            Assert.That(slider.Path.ControlPoints[3].Position, Is.EqualTo(perfectMiddle));
            Assert.That(slider.Path.ControlPoints[1].Position, Is.Not.EqualTo(new Vector2(70, 80)));
        }

        [Test]
        public void TestClassicRetractDeletesInheritedHandles()
        {
            var slider = new Slider
            {
                Path = new SliderPath(new[]
                {
                    new PathControlPoint(Vector2.Zero, PathType.BEZIER),
                    new PathControlPoint(new Vector2(40, 60)),
                    new PathControlPoint(new Vector2(100, 0), PathType.LINEAR),
                    new PathControlPoint(new Vector2(150, 40)),
                    new PathControlPoint(new Vector2(220, 0), PathType.LINEAR),
                })
            };

            Assert.That(ClassicSliderCurveOperations.TryRetractHandles(slider, new[] { slider.Path.ControlPoints[2] }), Is.True);
            Assert.That(slider.Path.ControlPoints.Count, Is.EqualTo(3));
            Assert.That(slider.Path.ControlPoints.All(p => p.Type.HasValue), Is.True);
        }

        [Test]
        public void TestClassicLinearSimplify()
        {
            var simplifySlider = new Slider
            {
                Path = new SliderPath(new[]
                {
                    new PathControlPoint(Vector2.Zero, PathType.LINEAR),
                    new PathControlPoint(new Vector2(40, 0)),
                    new PathControlPoint(new Vector2(100, 0)),
                    new PathControlPoint(new Vector2(180, 0), PathType.LINEAR),
                })
            };
            Assert.That(ClassicSliderCurveOperations.TrySimplifyLinear(simplifySlider,
                new[] { simplifySlider.Path.ControlPoints[0] }), Is.True);
            Assert.That(simplifySlider.Path.ControlPoints.Count, Is.EqualTo(2));
        }

        [Test]
        public void TestClosedRoundTripAndSplit()
        {
            var path = curve();
            path.Anchors.Add(new CurveAnchor(new Vector2(0, 120)));
            path.Closed = true;
            var native = new SliderPath(path.ToControlPoints().ToArray());
            Assert.That(AnchorPath.TryRead(native, out var read), Is.True);
            Assert.That(read.Closed, Is.True);
            Assert.That(read.Anchors.Count, Is.EqualTo(3));
            Assert.That(read.Split(2, 0.5f), Is.EqualTo(3));
            close(read.Evaluate(3, 1), Vector2.Zero);
        }

        [Test]
        public void TestSessionRejectsStaleGeometry()
        {
            var path = curve();
            path.Anchors[0].Kind = AnchorKind.AutoSmooth;
            var slider = new Slider { Path = new SliderPath(path.ToControlPoints().ToArray()) };
            AnchorEditSession.Remember(slider, path);
            Assert.That(AnchorEditSession.TryRead(slider, out var remembered), Is.True);
            Assert.That(remembered.Anchors[0].Kind, Is.EqualTo(AnchorKind.AutoSmooth));
            slider.Path.ControlPoints[1].Position += new Vector2(10, 0);
            Assert.That(AnchorEditSession.TryRead(slider, out var updated), Is.True);
            close(updated.Anchors[0].Outgoing, path.Anchors[0].Outgoing + new Vector2(10, 0));
            Assert.That(updated.Anchors[0].Kind, Is.Not.EqualTo(AnchorKind.AutoSmooth));
        }

        [Test]
        public void TestOpenContoursRejectedByBoolean()
        {
            var slider = new Slider { Path = new SliderPath(curve().ToControlPoints().ToArray()) };
            Assert.Throws<ArgumentException>(() => CurveGeometryOperations.Execute(new[] { slider, slider }, CurveOperation.Union, 8));
            Assert.That(CurveGeometryOperations.Execute(new[] { slider }, CurveOperation.StrokeOutline, 8), Is.Not.Empty);
        }

        [Test]
        public void TestInvalidGeometryAndAnchorLimit()
        {
            var path = curve();
            path.Anchors[0].Incoming = new Vector2(float.NaN, 0);
            Assert.That(path.IsFinite(), Is.False);
            path = curve();
            while (path.Anchors.Count < AnchorPath.MaxAnchors) path.Anchors.Add(new CurveAnchor(new Vector2(path.Anchors.Count, 0)));
            Assert.That(path.Split(0, 0.5f), Is.EqualTo(-1));
        }

        [TestCase(CurveOperation.Union, 1)]
        [TestCase(CurveOperation.Intersection, 1)]
        [TestCase(CurveOperation.Difference, 1)]
        [TestCase(CurveOperation.Xor, 2)]
        public void TestRectangleBooleanContours(CurveOperation operation, int count)
        {
            Slider rectangle(float x) => new Slider
            {
                Position = new Vector2(x, 0),
                Path = new SliderPath(PathType.LINEAR, new[]
                {
                    Vector2.Zero, new Vector2(100, 0), new Vector2(100, 100), new Vector2(0, 100), Vector2.Zero
                })
            };
            var first = rectangle(0);
            var second = rectangle(50);
            var result = CurveGeometryOperations.Execute(new[] { first, second }, operation, 8);
            Assert.That(result.Count, Is.EqualTo(count));
            Assert.That(result.All(p => p[0] == p[^1]), Is.True);
            Assert.That(first.Path.ControlPoints.Count, Is.EqualTo(5));
            Assert.That(second.Position.X, Is.EqualTo(50));
        }
    }
}
