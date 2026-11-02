// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using Clipper2Lib;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit.Curves
{
    public enum CurveOperation { Union, Intersection, Difference, Xor, Offset, StrokeOutline }

    public static class CurveGeometryOperations
    {
        /// <summary>Boolean operations on closed rendered centreline contours, never on slider hitbox discs.</summary>
        public static List<List<Vector2>> Execute(IReadOnlyList<Slider> sliders, CurveOperation operation, double offset)
        {
            if (!double.IsFinite(offset) || Math.Abs(offset) > 1000000)
                throw new ArgumentException("Offset must be finite and within 1,000,000 pixels.");

            bool unary = operation == CurveOperation.Offset || operation == CurveOperation.StrokeOutline;
            if (sliders.Count < (unary ? 1 : 2) || sliders.Count > 16)
                throw new ArgumentException(unary ? "Select 1 to 16 sliders." : "Select 2 to 16 sliders. Difference uses the earliest slider minus the others.");
            var contours = new PathsD();
            var closedContours = new List<bool>();
            foreach (var slider in sliders)
            {
                var vertices = new List<Vector2>();
                slider.Path.GetPathToProgress(vertices, 0, 1);
                if (vertices.Count < 2 || vertices.Count > 20000)
                    throw new ArgumentException("Input path must have 2 to 20,000 rendered vertices. Simplify very dense paths first.");
                bool closed = Vector2.DistanceSquared(vertices[0], vertices[^1]) < 0.01f;
                if (!closed && operation != CurveOperation.StrokeOutline)
                    throw new ArgumentException("Boolean/offset operations require closed visible contours. Close the path and use its full length first.");
                if (closed) vertices.RemoveAt(vertices.Count - 1);
                var contour = new PathD();
                foreach (var v in vertices)
                {
                    var p = v + slider.Position;
                    if (!float.IsFinite(p.X) || !float.IsFinite(p.Y) || Math.Abs(p.X) > 1000000 || Math.Abs(p.Y) > 1000000)
                        throw new ArgumentException("Input contains non-finite or excessively large coordinates.");
                    contour.Add(new PointD(p.X, p.Y));
                }
                contours.Add(contour);
                closedContours.Add(closed);
            }

            PathsD output;
            if (unary)
            {
                if (operation == CurveOperation.StrokeOutline && offset <= 0)
                    throw new ArgumentException("Stroke outline needs a positive half-width.");
                output = new PathsD();
                // Each slider is a separate filled input. Preserve holes created by each offset operation.
                for (int i = 0; i < contours.Count; i++)
                {
                    var contour = contours[i];
                    if (Clipper.Area(contour) < 0) contour.Reverse();
                    output.AddRange(Clipper.InflatePaths(new PathsD { contour }, offset, JoinType.Round,
                        operation == CurveOperation.Offset ? EndType.Polygon : closedContours[i] ? EndType.Joined : EndType.Round, 2, 3));
                }
            }
            else
            {
                output = new PathsD { contours[0] };
                ClipType type = operation switch
                {
                    CurveOperation.Union => ClipType.Union,
                    CurveOperation.Intersection => ClipType.Intersection,
                    CurveOperation.Difference => ClipType.Difference,
                    _ => ClipType.Xor
                };
                for (int i = 1; i < contours.Count; i++)
                    output = Clipper.BooleanOp(type, output, new PathsD { contours[i] }, FillRule.EvenOdd, 3);
            }
            if (output.Count > 128 || output.Sum(p => p.Count) > 20000)
                throw new ArgumentException("Result is too complex (maximum 128 contours / 20,000 vertices).");
            var result = new List<List<Vector2>>();
            foreach (var contour in output.Where(p => p.Count >= 3))
            {
                var points = contour.Select(p => new Vector2((float)p.x, (float)p.y)).ToList();
                points.Add(points[0]);
                result.Add(points);
            }
            return result;
        }
    }
}
