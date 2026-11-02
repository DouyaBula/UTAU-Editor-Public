// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Utils;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit.Curves
{
    /// <summary>
    /// Editing operations for the native control-point representation used by the classic slider editor.
    ///
    /// A point with a <see cref="PathControlPoint.Type"/> starts a segment (an uninherited anchor).
    /// Points with a null type are inherited points belonging to that segment. Classic operations are
    /// therefore anchor operations: inherited points are handles/geometry, never accidental anchors.
    /// </summary>
    public static class ClassicSliderCurveOperations
    {
        public static bool TryStraighten(Slider slider, IReadOnlyCollection<PathControlPoint> selected)
        {
            bool changed = false;
            var points = slider.Path.ControlPoints;

            foreach (int anchorIndex in selectedAnchorIndices(points, selected).OrderBy(i => i))
            {
                int end = segmentEnd(points, anchorIndex);
                if (end - anchorIndex < 2)
                    continue;

                Vector2 start = points[anchorIndex].Position;
                Vector2 finish = points[end].Position;
                for (int i = anchorIndex + 1; i < end; i++)
                    changed |= setPosition(points[i], Vector2.Lerp(start, finish, (i - anchorIndex) / (float)(end - anchorIndex)));
            }

            return changed;
        }

        /// <summary>
        /// Removes editable inherited handles around selected anchors. Perfect-curve and Catmull
        /// points are retained when they are structural (the former needs its middle point to define
        /// the arc, while the latter uses inherited points as knots rather than handles).
        /// </summary>
        public static bool TryRetractHandles(Slider slider, IReadOnlyCollection<PathControlPoint> selected)
        {
            bool changed = false;
            var points = slider.Path.ControlPoints;

            foreach (var anchor in selected.Where(p => p.Type.HasValue).ToArray())
            {
                int index = findIndex(points, anchor);
                if (index < 0)
                    continue;

                int outgoingEnd = segmentEnd(points, index);
                changed |= retractSegment(points, index, outgoingEnd);

                if (index > 0)
                {
                    int incomingStart = segmentStart(points, index - 1);
                    // The selected anchor remains in the list, so its current index is still valid.
                    changed |= retractSegment(points, incomingStart, index);
                }
            }

            return changed;
        }

        /// <summary>
        /// Smooths selected anchors using the actual tangent of each neighbouring segment. Only null
        /// inherited points are moved as handles. If a side has no handle, one is inserted rather than
        /// repurposing another uninherited anchor. Perfect-curve middle points are used to calculate
        /// a tangent but are never moved by this operation.
        /// </summary>
        public static bool TrySmooth(Slider slider, IReadOnlyCollection<PathControlPoint> selected, bool symmetric)
        {
            bool changed = false;
            var points = slider.Path.ControlPoints;

            foreach (var anchor in selected.Where(p => p.Type.HasValue).ToArray())
            {
                int index = findIndex(points, anchor);
                if (index < 0)
                    continue;

                int outgoingEnd = segmentEnd(points, index);
                int incomingStart = index > 0 ? segmentStart(points, index - 1) : -1;
                PathType outgoingType = points[index].Type ?? PathType.LINEAR;
                PathType? incomingType = incomingStart >= 0 ? points[incomingStart].Type : null;

                Vector2 outgoingTangent = segmentTangent(points, index, outgoingEnd, atStart: true);
                Vector2 incomingTangent = incomingStart >= 0
                    ? segmentTangent(points, incomingStart, index, atStart: false)
                    : outgoingTangent;

                // Prefer a native perfect-curve tangent at a mixed boundary. Otherwise average the
                // two sides, which produces a true C1 direction when both are ordinary splines.
                Vector2 tangent = outgoingType.Type == SplineType.PerfectCurve
                    ? outgoingTangent
                    : incomingType?.Type == SplineType.PerfectCurve
                        ? incomingTangent
                        : safeNormal(incomingTangent + outgoingTangent, outgoingTangent);

                bool incomingEditable = incomingStart >= 0 && index - incomingStart > 1
                                         && points[index - 1].Type == null
                                         && incomingType is PathType incomingPathType && editableHandleType(incomingPathType);
                bool outgoingEditable = outgoingEnd - index > 1
                                         && points[index + 1].Type == null
                                         && editableHandleType(outgoingType);

                float incomingLength = incomingEditable
                    ? Vector2.Distance(points[index - 1].Position, points[index].Position)
                    : incomingStart >= 0 ? Vector2.Distance(points[incomingStart].Position, points[index].Position) / 3 : 0;
                float outgoingLength = outgoingEditable
                    ? Vector2.Distance(points[index + 1].Position, points[index].Position)
                    : outgoingEnd > index ? Vector2.Distance(points[outgoingEnd].Position, points[index].Position) / 3 : 0;

                if (symmetric && incomingLength > 0 && outgoingLength > 0)
                    incomingLength = outgoingLength = MathF.Min(incomingLength, outgoingLength);

                if (incomingEditable)
                    changed |= setPosition(points[index - 1], points[index].Position - tangent * incomingLength);
                else if (incomingStart >= 0 && incomingType is PathType incomingTypeValue && editableHandleType(incomingTypeValue) && incomingLength > 0)
                {
                    points.Insert(index, new PathControlPoint(points[index].Position - tangent * incomingLength));
                    changed = true;
                    index++;
                    outgoingEnd++;
                }

                // Re-read the outgoing side after a possible insertion before the anchor.
                outgoingEditable = outgoingEnd - index > 1
                                   && points[index + 1].Type == null
                                   && points[index].Type is PathType currentPathType && editableHandleType(currentPathType);

                if (outgoingEditable)
                    changed |= setPosition(points[index + 1], points[index].Position + tangent * outgoingLength);
                else if (outgoingEnd > index && editableHandleType(points[index].Type ?? PathType.LINEAR) && outgoingLength > 0)
                {
                    points.Insert(index + 1, new PathControlPoint(points[index].Position + tangent * outgoingLength));
                    changed = true;
                }
            }

            return changed;
        }

        public static bool TryAlign(Slider slider, IReadOnlyCollection<PathControlPoint> selected, bool x)
        {
            var points = selectedAnchors(slider.Path.ControlPoints, selected).ToArray();
            if (points.Length < 2)
                return false;

            float value = x ? points.Average(p => p.Position.X) : points.Average(p => p.Position.Y);
            bool changed = false;
            foreach (var point in points)
                changed |= setPosition(point, x ? new Vector2(value, point.Position.Y) : new Vector2(point.Position.X, value));
            return changed;
        }

        public static bool TryDistribute(Slider slider, IReadOnlyCollection<PathControlPoint> selected, bool x)
        {
            var points = selectedAnchors(slider.Path.ControlPoints, selected)
                .OrderBy(p => x ? p.Position.X : p.Position.Y)
                .ToArray();
            if (points.Length < 3)
                return false;

            float start = x ? points[0].Position.X : points[0].Position.Y;
            float end = x ? points[^1].Position.X : points[^1].Position.Y;
            bool changed = false;
            for (int i = 1; i < points.Length - 1; i++)
            {
                float value = start + (end - start) * i / (points.Length - 1);
                changed |= setPosition(points[i], x ? new Vector2(value, points[i].Position.Y) : new Vector2(points[i].Position.X, value));
            }
            return changed;
        }

        public static bool TryMirror(Slider slider, IReadOnlyCollection<PathControlPoint> selected, bool x)
        {
            var points = selectedAnchors(slider.Path.ControlPoints, selected).ToArray();
            if (points.Length == 0)
                return false;

            float centre = x ? points.Average(p => p.Position.X) : points.Average(p => p.Position.Y);
            bool changed = false;
            foreach (var point in points)
            {
                Vector2 mirrored = x
                    ? new Vector2(2 * centre - point.Position.X, point.Position.Y)
                    : new Vector2(point.Position.X, 2 * centre - point.Position.Y);
                changed |= setPosition(point, mirrored);
            }
            return changed;
        }

        /// <summary>
        /// Removes inherited points that are visually redundant on selected linear segments. This is
        /// the destructive counterpart to adding a midpoint and mirrors the “Simplify path” command
        /// found in vector editors, while deliberately leaving Bézier/Perfect/Catmull geometry intact.
        /// </summary>
        public static bool TrySimplifyLinear(Slider slider, IReadOnlyCollection<PathControlPoint> selected, float tolerance = 1.5f)
        {
            bool changed = false;
            var points = slider.Path.ControlPoints;

            foreach (var anchor in selected.Where(p => p.Type?.Type == SplineType.Linear).ToArray())
            {
                int index = findIndex(points, anchor);
                if (index < 0)
                    continue;

                int end = segmentEnd(points, index);
                if (end - index < 2)
                    continue;

                Vector2 start = points[index].Position;
                Vector2 finish = points[end].Position;
                Vector2 line = finish - start;
                float lineLengthSquared = line.LengthSquared;
                if (lineLengthSquared < 0.01f)
                    continue;

                for (int i = end - 1; i > index; i--)
                {
                    if (points[i].Type.HasValue)
                        continue;

                    float projection = Vector2.Dot(points[i].Position - start, line) / lineLengthSquared;
                    projection = Math.Clamp(projection, 0, 1);
                    Vector2 nearest = start + line * projection;
                    if (Vector2.DistanceSquared(points[i].Position, nearest) <= tolerance * tolerance)
                    {
                        points.RemoveAt(i);
                        changed = true;
                    }
                }
            }

            return changed;
        }

        /// <summary>
        /// Trims selected uninherited anchors into a bevel or replaces their local corner with a
        /// cubic round. The generic path transform supports mixed native segments. It uses one typed
        /// entry point (no same-position inherited duplicate) and a typed exit point carrying the
        /// original outgoing segment type.
        /// </summary>
        public static bool TryRoundOrBevel(Slider slider, IReadOnlyCollection<PathControlPoint> selected, float amount, bool bevel,
                                           bool keepDuration, out string error)
        {
            error = string.Empty;
            var source = slider.Path.ControlPoints;
            var chosen = selectedIndices(source, selected)
                .Where(i => i > 0 && i < source.Count - 1 && source[i].Type.HasValue)
                .Distinct()
                .OrderByDescending(i => i)
                .ToArray();
            if (chosen.Length == 0)
            {
                error = "Select an interior uninherited anchor (endpoints and inherited handles are not corner targets).";
                return false;
            }

            double originalDistance = slider.Path.Distance;
            double originalVelocity = slider.SliderVelocityMultiplier;
            double? expectedDistance = slider.Path.ExpectedDistance.Value;
            var originalPoints = source.Select(p => new PathControlPoint(p.Position, p.Type)).ToArray();

            // Preserve the established anchor-aware Bézier corner result where possible. This also
            // keeps the inner-handle-only convention used by the pen editor.
            if (AnchorPath.TryRead(slider.Path, out var bezierPath) && chosen.All(i => i % 3 == 0))
            {
                bool transformed = false;
                foreach (int index in chosen.Select(i => i / 3))
                    transformed |= bezierPath.RoundCorner(index, Math.Clamp(amount, 0.01f, 0.49f), bevel);

                if (!transformed)
                {
                    error = "The selected Bézier corner cannot be transformed.";
                    return false;
                }

                bezierPath.WriteTo(slider.Path);
                return finish(slider, originalPoints, expectedDistance, originalDistance, originalVelocity, keepDuration, transformed, out error);
            }

            var points = originalPoints.ToList();
            float t = Math.Clamp(amount, 0.01f, 0.49f);

            foreach (int index in chosen)
            {
                if (index <= 0 || index >= points.Count - 1 || !points[index].Type.HasValue)
                    continue;

                Vector2 previous = points[index - 1].Position;
                Vector2 pivot = points[index].Position;
                Vector2 next = points[index + 1].Position;
                PathType outgoingType = points[index].Type ?? PathType.LINEAR;
                Vector2 entry = Vector2.Lerp(previous, pivot, 1 - t);
                Vector2 exit = Vector2.Lerp(pivot, next, t);

                if (bevel)
                {
                    points[index] = new PathControlPoint(entry, PathType.LINEAR);
                    points.Insert(index + 1, new PathControlPoint(exit, outgoingType));
                }
                else
                {
                    Vector2 incomingTangent = safeNormal(pivot - previous, next - pivot);
                    Vector2 outgoingTangent = safeNormal(next - pivot, pivot - previous);
                    float handleLength = Vector2.Distance(entry, exit) / 3;
                    points[index] = new PathControlPoint(entry, PathType.BEZIER);
                    points.Insert(index + 1, new PathControlPoint(entry + incomingTangent * handleLength));
                    points.Insert(index + 2, new PathControlPoint(exit - outgoingTangent * handleLength));
                    points.Insert(index + 3, new PathControlPoint(exit, outgoingType));
                }
            }

            slider.Path.ControlPoints.ReplaceRange(0, slider.Path.ControlPoints.Count, points);
            bool changed = points.Count != originalPoints.Length || points.Where((p, i) => p.Position != originalPoints[i].Position || p.Type != originalPoints[i].Type).Any();
            return finish(slider, originalPoints, expectedDistance, originalDistance, originalVelocity, keepDuration, changed, out error);
        }

        private static IEnumerable<PathControlPoint> selectedAnchors(IList<PathControlPoint> points, IReadOnlyCollection<PathControlPoint> selected)
            => selected.Where(p => p.Type.HasValue && findIndex(points, p) >= 0);

        private static IEnumerable<int> selectedAnchorIndices(IList<PathControlPoint> points, IReadOnlyCollection<PathControlPoint> selected)
            => selected.Where(p => p.Type.HasValue).Select(p => findIndex(points, p)).Where(i => i >= 0).Distinct();

        private static IEnumerable<int> selectedIndices(IList<PathControlPoint> points, IReadOnlyCollection<PathControlPoint> selected)
            => selected.Select(p => findIndex(points, p)).Where(i => i >= 0);

        private static bool retractSegment(IList<PathControlPoint> points, int start, int end)
        {
            if (end - start <= 1)
                return false;

            PathType type = points[start].Type ?? inheritedType(points, start);
            int count = end - start - 1;
            if (type.Type == SplineType.PerfectCurve)
            {
                // The sole inherited point is the arc's defining point, not a free handle.
                if (count <= 1)
                    return false;
                for (int i = 0; i < count - 1; i++)
                    points.RemoveAt(start + 2);
                return true;
            }

            if (type.Type == SplineType.Catmull)
                return false;

            for (int i = 0; i < count; i++)
                points.RemoveAt(start + 1);
            return true;
        }

        private static int findIndex(IList<PathControlPoint> points, PathControlPoint target)
        {
            for (int i = 0; i < points.Count; i++)
                if (ReferenceEquals(points[i], target))
                    return i;
            return -1;
        }

        private static int segmentStart(IList<PathControlPoint> points, int index)
        {
            for (int i = index; i >= 0; i--)
                if (points[i].Type.HasValue)
                    return i;
            return 0;
        }

        private static int segmentEnd(IList<PathControlPoint> points, int index)
        {
            for (int i = index + 1; i < points.Count; i++)
                if (points[i].Type.HasValue)
                    return i;
            return points.Count - 1;
        }

        private static PathType inheritedType(IList<PathControlPoint> points, int index)
        {
            for (int i = index; i >= 0; i--)
                if (points[i].Type is PathType type)
                    return type;
            return PathType.LINEAR;
        }

        private static bool editableHandleType(PathType type)
            => type.Type == SplineType.Linear || type.Type == SplineType.BSpline;

        private static Vector2 segmentTangent(IList<PathControlPoint> points, int start, int end, bool atStart)
        {
            if (start < 0 || end <= start)
                return Vector2.UnitX;

            PathType type = points[start].Type ?? inheritedType(points, start);
            if (type.Type == SplineType.PerfectCurve && end - start == 2)
            {
                Vector2[] arcPoints = { points[start].Position, points[start + 1].Position, points[end].Position };
                var arc = new CircularArcProperties(arcPoints);
                if (arc.IsValid)
                {
                    Vector2 radius = (atStart ? points[start].Position : points[end].Position) - arc.Centre;
                    Vector2 middleRadius = points[start + 1].Position - arc.Centre;
                    float cross = radius.X * middleRadius.Y - radius.Y * middleRadius.X;
                    if (MathF.Abs(cross) > 0.0001f)
                    {
                        Vector2 tangent = new Vector2(-radius.Y, radius.X);
                        if (cross < 0)
                            tangent = -tangent;
                        return tangent.Normalized();
                    }
                }
            }

            if (atStart)
            {
                for (int i = start + 1; i < end; i++)
                    if (points[i].Type == null)
                        return safeNormal(points[i].Position - points[start].Position, points[end].Position - points[start].Position);
                return safeNormal(points[end].Position - points[start].Position, Vector2.UnitX);
            }

            for (int i = end - 1; i > start; i--)
                if (points[i].Type == null)
                    return safeNormal(points[end].Position - points[i].Position, points[end].Position - points[start].Position);
            return safeNormal(points[end].Position - points[start].Position, Vector2.UnitX);
        }

        private static Vector2 safeNormal(Vector2 primary, Vector2 fallback)
        {
            if (primary.LengthSquared >= 0.0001f)
                return primary.Normalized();
            if (fallback.LengthSquared >= 0.0001f)
                return fallback.Normalized();
            return Vector2.UnitX;
        }

        private static bool setPosition(PathControlPoint point, Vector2 position)
        {
            if (point.Position == position)
                return false;
            point.Position = position;
            return true;
        }

        private static bool finish(Slider slider, PathControlPoint[] originalPoints, double? expectedDistance,
                                   double originalDistance, double originalVelocity, bool keepDuration,
                                   bool changed, out string error)
        {
            error = string.Empty;
            slider.Path.ExpectedDistance.Value = expectedDistance;

            if (!changed)
                return false;

            if (keepDuration && originalDistance > 0)
            {
                double adjustedVelocity = originalVelocity * slider.Path.Distance / originalDistance;
                if (adjustedVelocity < slider.SliderVelocityMultiplierBindable.MinValue
                    || adjustedVelocity > slider.SliderVelocityMultiplierBindable.MaxValue)
                {
                    restore(slider, originalPoints, expectedDistance, originalVelocity);
                    error = "Keeping duration would exceed osu!'s SV limits.";
                    return false;
                }
                slider.SliderVelocityMultiplier = adjustedVelocity;
            }

            return true;
        }

        private static void restore(Slider slider, PathControlPoint[] points, double? expectedDistance, double velocity)
        {
            slider.Path.ControlPoints.ReplaceRange(0, slider.Path.ControlPoints.Count, points);
            slider.Path.ExpectedDistance.Value = expectedDistance;
            slider.SliderVelocityMultiplier = velocity;
        }
    }
}
