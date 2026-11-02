// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit.Curves
{
    public enum AnchorKind { Corner, Smooth, Symmetric, AutoSmooth }
    public enum CurveMirror { None, Horizontal, Vertical }

    /// <summary>Editor-only view of native piecewise cubic Beziers. Handles are offsets from the anchor.</summary>
    public class CurveAnchor
    {
        public Vector2 Position;
        public Vector2 Incoming;
        public Vector2 Outgoing;
        public AnchorKind Kind;
        public CurveAnchor(Vector2 position) => Position = position;
        public CurveAnchor Clone() => (CurveAnchor)MemberwiseClone();
    }

    public class AnchorPath
    {
        // Bound interactive complexity independently of the traditional dense-control-point renderer.
        public const int MaxAnchors = 256;
        public List<CurveAnchor> Anchors { get; } = new List<CurveAnchor>();
        public bool Closed;
        public int SegmentCount => Closed ? Anchors.Count : Math.Max(0, Anchors.Count - 1);

        public AnchorPath Clone()
        {
            var copy = new AnchorPath { Closed = Closed };
            copy.Anchors.AddRange(Anchors.Select(a => a.Clone()));
            return copy;
        }

        public Vector2 Evaluate(int segment, float t)
        {
            var a = Anchors[segment];
            var b = Anchors[(segment + 1) % Anchors.Count];
            float u = 1 - t;
            return u * u * u * a.Position + 3 * u * u * t * (a.Position + a.Outgoing)
                   + 3 * u * t * t * (b.Position + b.Incoming) + t * t * t * b.Position;
        }

        /// <summary>
        /// Writes this path to an existing native path, reusing its control-point objects whenever the
        /// topology is unchanged. This is the hot path used while dragging a pen handle.
        /// </summary>
        public void WriteTo(SliderPath target)
        {
            int expectedCount = Anchors.Count == 0 ? 0 : SegmentCount * 3 + 1;
            if (target.ControlPoints.Count != expectedCount)
            {
                // ReplaceRange emits one collection notification with the final topology. A clear
                // followed by AddRange briefly exposes an empty path to editor blueprints, which can
                // make them rebuild the classic control-point editor in the middle of a pen gesture.
                target.ControlPoints.ReplaceRange(0, target.ControlPoints.Count, ToControlPoints());
                return;
            }

            int index = 0;
            for (int i = 0; i < SegmentCount; i++)
            {
                var a = Anchors[i];
                var b = Anchors[(i + 1) % Anchors.Count];
                update(target.ControlPoints[index++], a.Position, PathType.BEZIER);
                update(target.ControlPoints[index++], a.Position + a.Outgoing, null);
                update(target.ControlPoints[index++], b.Position + b.Incoming, null);
            }

            if (Anchors.Count > 0)
                update(target.ControlPoints[index], Closed ? Anchors[0].Position : Anchors[^1].Position, null);

            static void update(PathControlPoint point, Vector2 position, PathType? type)
            {
                if (point.Position != position)
                    point.Position = position;
                if (point.Type != type)
                    point.Type = type;
            }
        }

        public List<PathControlPoint> ToControlPoints()
        {
            var result = new List<PathControlPoint>();
            for (int i = 0; i < SegmentCount; i++)
            {
                var a = Anchors[i];
                var b = Anchors[(i + 1) % Anchors.Count];
                result.Add(new PathControlPoint(a.Position, PathType.BEZIER));
                result.Add(new PathControlPoint(a.Position + a.Outgoing));
                result.Add(new PathControlPoint(b.Position + b.Incoming));
            }
            if (Anchors.Count > 0)
                result.Add(new PathControlPoint(Closed ? Anchors[0].Position : Anchors[^1].Position));
            return result;
        }

        /// <summary>Read without writing anything back. Only native pure Bézier segments are accepted.</summary>
        public static bool TryRead(SliderPath source, out AnchorPath result)
        {
            result = new AnchorPath();
            var points = source.ControlPoints;
            if (points.Count < 2 || points.Count > MaxAnchors * 3 + 1)
                return false;
            result.Anchors.Add(new CurveAnchor(points[0].Position));
            int start = 0;
            for (int end = 1; end < points.Count; end++)
            {
                if (points[end].Type == null && end != points.Count - 1)
                    continue;
                var type = points[start].Type ?? PathType.LINEAR;
                int count = end - start + 1;
                if (type.Type == SplineType.BSpline && type.Degree == null && count <= 4)
                {
                    var a = result.Anchors[^1];
                    var b = new CurveAnchor(points[end].Position);
                    if (count == 3)
                    {
                        a.Outgoing = (points[start + 1].Position - a.Position) * (2f / 3);
                        b.Incoming = (points[start + 1].Position - b.Position) * (2f / 3);
                    }
                    else if (count == 4)
                    {
                        a.Outgoing = points[start + 1].Position - a.Position;
                        b.Incoming = points[start + 2].Position - b.Position;
                    }
                    result.Anchors.Add(b);
                }
                else
                    return false;
                start = end;
            }
            if (result.Anchors.Count > 2 && result.Anchors[0].Position == result.Anchors[^1].Position)
            {
                result.Closed = true;
                result.Anchors[0].Incoming = result.Anchors[^1].Incoming;
                result.Anchors.RemoveAt(result.Anchors.Count - 1);
            }
            foreach (var a in result.Anchors)
            {
                if (a.Incoming.LengthSquared > 0.001f && a.Outgoing.LengthSquared > 0.001f
                    && Vector2.Dot(Vector2.Normalize(a.Incoming), Vector2.Normalize(a.Outgoing)) < -0.9999f)
                    a.Kind = Math.Abs(a.Incoming.Length - a.Outgoing.Length) < 0.01f ? AnchorKind.Symmetric : AnchorKind.Smooth;
            }
            return result.Anchors.Count <= MaxAnchors;
        }

        public void SetKind(int index, AnchorKind kind)
        {
            var a = Anchors[index];
            a.Kind = kind;
            if (kind == AnchorKind.Corner)
                return; // A cusp has independent handles, not necessarily absent handles.
            if (kind == AnchorKind.AutoSmooth || (a.Incoming.LengthSquared + a.Outgoing.LengthSquared < 0.001f))
                autoSmooth(index);
            else
                MoveHandle(index, a.Outgoing.LengthSquared > 0.001f,
                    a.Outgoing.LengthSquared > 0.001f ? a.Outgoing : a.Incoming, false);
        }

        public void RecalculateAutoSmooth()
        {
            for (int i = 0; i < Anchors.Count; i++)
                if (Anchors[i].Kind == AnchorKind.AutoSmooth)
                    autoSmooth(i);
        }

        private void autoSmooth(int index)
        {
            var a = Anchors[index];
            var prev = Anchors[index == 0 ? (Closed ? Anchors.Count - 1 : 0) : index - 1];
            var next = Anchors[index == Anchors.Count - 1 ? (Closed ? 0 : index) : index + 1];
            var direction = next.Position - prev.Position;
            if (direction.LengthSquared < 0.0001f)
            {
                a.Incoming = a.Outgoing = Vector2.Zero;
                return;
            }
            direction.Normalize();
            a.Incoming = -direction * Vector2.Distance(a.Position, prev.Position) / 3;
            a.Outgoing = direction * Vector2.Distance(a.Position, next.Position) / 3;
            if (a.Kind == AnchorKind.Symmetric)
                a.Incoming = -a.Outgoing;
        }

        public void MoveHandle(int index, bool outgoing, Vector2 offset, bool independent)
        {
            var a = Anchors[index];
            if (independent || a.Kind == AnchorKind.AutoSmooth)
                a.Kind = independent ? AnchorKind.Corner : AnchorKind.Smooth;
            var opposite = outgoing ? a.Incoming : a.Outgoing;
            if (a.Kind == AnchorKind.Symmetric)
                opposite = -offset;
            else if (a.Kind == AnchorKind.Smooth && offset.LengthSquared > 0.0001f)
                opposite = -Vector2.Normalize(offset) * opposite.Length;
            if (outgoing) { a.Outgoing = offset; a.Incoming = opposite; }
            else { a.Incoming = offset; a.Outgoing = opposite; }
        }

        /// <summary>de Casteljau subdivision preserves the curve exactly.</summary>
        public int Split(int segment, float t)
        {
            if (Anchors.Count >= MaxAnchors) return -1;
            t = Math.Clamp(t, 0.001f, 0.999f);
            var a = Anchors[segment];
            var b = Anchors[(segment + 1) % Anchors.Count];
            var p = Vector2.Lerp(a.Position, a.Position + a.Outgoing, t);
            var q = Vector2.Lerp(a.Position + a.Outgoing, b.Position + b.Incoming, t);
            var r = Vector2.Lerp(b.Position + b.Incoming, b.Position, t);
            var s = Vector2.Lerp(p, q, t);
            var u = Vector2.Lerp(q, r, t);
            var v = Vector2.Lerp(s, u, t);
            a.Outgoing = p - a.Position;
            b.Incoming = r - b.Position;
            a.Kind = b.Kind = AnchorKind.Corner;
            Anchors.Insert(segment + 1, new CurveAnchor(v) { Incoming = s - v, Outgoing = u - v, Kind = AnchorKind.Smooth });
            return segment + 1;
        }

        public void Reverse()
        {
            Anchors.Reverse();
            foreach (var a in Anchors)
                (a.Incoming, a.Outgoing) = (a.Outgoing, a.Incoming);
        }

        /// <summary>Trim both adjacent cubics and insert a tangent-continuous rounded or straight bevel segment.</summary>
        public bool RoundCorner(int index, float amount, bool bevel)
        {
            if (Anchors.Count >= MaxAnchors || (!Closed && (index == 0 || index == Anchors.Count - 1)) || Anchors.Count < 3)
                return false;
            int prevIndex = (index + Anchors.Count - 1) % Anchors.Count;
            var prev = Anchors[prevIndex];
            var a = Anchors[index];
            var next = Anchors[(index + 1) % Anchors.Count];
            // amount is a fraction of each neighbouring segment, capped below the midpoint.
            float t = Math.Clamp(amount, 0.01f, 0.49f);
            var left = trim(prev.Position, prev.Position + prev.Outgoing, a.Position + a.Incoming, a.Position, 1 - t);
            var right = trim(a.Position, a.Position + a.Outgoing, next.Position + next.Incoming, next.Position, t);
            prev.Outgoing = left[0] - prev.Position;
            next.Incoming = right[2] - next.Position;
            var entry = left[5];
            var exit = right[5];
            var tangentIn = entry - left[3];
            var tangentOut = right[4] - exit;
            float length = Vector2.Distance(entry, exit) / 3;
            a.Position = entry;
            // A rounded/beveled corner exposes only the handles that face the
            // newly-created corner span. The handles on the outside of the
            // corner are deliberately retracted so the result reads as a clean
            // corner in pen mode instead of two extra outward-facing tangents.
            a.Incoming = Vector2.Zero;
            a.Outgoing = bevel || tangentIn.LengthSquared < 0.0001f ? Vector2.Zero : Vector2.Normalize(tangentIn) * length;
            a.Kind = AnchorKind.Corner;
            Anchors.Insert(index + 1, new CurveAnchor(exit)
            {
                Incoming = bevel || tangentOut.LengthSquared < 0.0001f ? Vector2.Zero : -Vector2.Normalize(tangentOut) * length,
                Outgoing = Vector2.Zero,
            });
            prev.Kind = next.Kind = AnchorKind.Corner;
            return true;
        }

        private static Vector2[] trim(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float t)
        {
            var p = Vector2.Lerp(a, b, t); var q = Vector2.Lerp(b, c, t); var r = Vector2.Lerp(c, d, t);
            var s = Vector2.Lerp(p, q, t); var u = Vector2.Lerp(q, r, t);
            return new[] { p, q, r, s, u, Vector2.Lerp(s, u, t) };
        }

        public static Vector2 Reflect(Vector2 position, CurveMirror axis, float centre)
            => axis == CurveMirror.Horizontal ? new Vector2(2 * centre - position.X, position.Y)
                : axis == CurveMirror.Vertical ? new Vector2(position.X, 2 * centre - position.Y) : position;

        public void Mirror(CurveMirror axis, float centre)
        {
            foreach (var a in Anchors)
            {
                a.Position = Reflect(a.Position, axis, centre);
                a.Incoming = Reflect(a.Incoming, axis, 0);
                a.Outgoing = Reflect(a.Outgoing, axis, 0);
            }
        }

        public bool IsFinite() => Anchors.All(a => finite(a.Position) && finite(a.Incoming) && finite(a.Outgoing));
        private static bool finite(Vector2 v) => float.IsFinite(v.X) && float.IsFinite(v.Y) && Math.Abs(v.X) < 1000000 && Math.Abs(v.Y) < 1000000;
    }
}
