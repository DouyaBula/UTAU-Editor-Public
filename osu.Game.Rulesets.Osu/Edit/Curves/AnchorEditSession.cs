// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit.Curves
{
    /// <summary>
    /// Session-only node constraints. Native control points remain authoritative; stale entries are never applied.
    /// No metadata is added to exported beatmaps. Reopening a map infers constraints from native geometry.
    /// </summary>
    public static class AnchorEditSession
    {
        private sealed class Entry
        {
            public readonly List<(PathControlPoint[] points, AnchorPath path)> History = new List<(PathControlPoint[], AnchorPath)>();
        }

        private static readonly ConditionalWeakTable<Slider, Entry> entries = new ConditionalWeakTable<Slider, Entry>();

        public static bool TryRead(Slider slider, out AnchorPath path)
        {
            if (entries.TryGetValue(slider, out var entry))
            {
                for (int i = entry.History.Count - 1; i >= 0; i--)
                {
                    var snapshot = entry.History[i];
                    if (snapshot.points.SequenceEqual(slider.Path.ControlPoints))
                    {
                        path = snapshot.path.Clone();
                        return true;
                    }
                }
            }
            return AnchorPath.TryRead(slider.Path, out path);
        }

        public static void Remember(Slider slider, AnchorPath path)
        {
            var entry = entries.GetValue(slider, _ => new Entry());
            entry.History.Add((slider.Path.ControlPoints.Select(p => new PathControlPoint(p.Position, p.Type)).ToArray(), path.Clone()));
            if (entry.History.Count > 16) entry.History.RemoveAt(0);
        }

        /// <summary>Validate geometry and duration/SV before touching the actual beatmap.</summary>
        public static bool TryApply(Slider slider, AnchorPath path, EditorBeatmap beatmap, bool preserveDuration, out string error,
                                    bool rebuildDefaults = true, bool notifyBeatmap = true, bool remember = true)
        {
            error = string.Empty;
            if (path.Anchors.Count < 2 || !path.IsFinite())
            {
                error = "A curve needs at least two finite anchors.";
                return false;
            }
            var origin = path.Anchors[0].Position;
            var points = path.ToControlPoints();
            foreach (var p in points) p.Position -= origin;
            var native = new SliderPath(points.ToArray());
            double length = native.CalculatedDistance;
            double velocity = slider.SliderVelocityMultiplier;
            if (!double.IsFinite(length) || length < 0.01)
            {
                error = "The resulting curve is too short.";
                return false;
            }
            if (preserveDuration && slider.Path.Distance > 0)
                velocity *= length / slider.Path.Distance;
            if (velocity < slider.SliderVelocityMultiplierBindable.MinValue || velocity > slider.SliderVelocityMultiplierBindable.MaxValue)
            {
                error = "Keeping duration would exceed osu!'s SV limits. Turn off Keep duration, or use a smaller edit.";
                return false;
            }
            slider.Position += origin;
            // Do not clear and re-add the points here.  SliderSelectionBlueprint listens to
            // ControlPoints.CollectionChanged and would observe a transient empty path, tear down
            // the anchor overlay, and recreate the classic control-point visualiser.  That is the
            // source of the apparent mode switch and the extra control points when dragging a handle.
            // Keep the existing objects for the hot path, and use one atomic collection replacement
            // when an edit changes the topology (insert/delete).
            if (slider.Path.ControlPoints.Count == points.Count)
            {
                for (int i = 0; i < points.Count; i++)
                {
                    slider.Path.ControlPoints[i].Position = points[i].Position;
                    slider.Path.ControlPoints[i].Type = points[i].Type;
                }
            }
            else
                slider.Path.ControlPoints.ReplaceRange(0, slider.Path.ControlPoints.Count, points);
            slider.Path.ExpectedDistance.Value = length;
            slider.SliderVelocityMultiplier = velocity;
            if (rebuildDefaults)
                slider.ApplyDefaults(beatmap.ControlPointInfo, beatmap.Difficulty);
            else
                slider.UpdateVelocity(beatmap.ControlPointInfo, beatmap.Difficulty);
            if (notifyBeatmap)
                beatmap.Update(slider);
            if (remember)
            {
                var normalised = path.Clone();
                foreach (var a in normalised.Anchors) a.Position -= origin;
                Remember(slider, normalised);
            }
            return true;
        }
    }
}
