// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Screens.Edit.Timing
{
    /// <summary>
    /// Translates a difficulty's musical timeline without changing its relative timing.
    /// </summary>
    public static class GlobalOffsetAdjustment
    {
        public static string? Validate(EditorBeatmap beatmap, int offset)
        {
            if (beatmap.HitObjects.Any(h => !validTime(h.StartTime + offset) || !validTime(h.GetEndTime() + offset)))
                return "This shift would move a hit object before 0 ms or beyond the supported time range. Nothing was moved.";

            if (beatmap.Bookmarks.Any(t => !validTime((double)t + offset))
                || beatmap.Breaks.Any(b => !validTime(b.StartTime + offset) || !validTime(b.EndTime + offset))
                || (beatmap.PreviewTime.Value >= 0 && !validTime((double)beatmap.PreviewTime.Value + offset)))
                return "This shift would move a bookmark, break or preview point outside the supported time range. Nothing was moved.";

            // Negative timing offsets are valid, unlike negative object start times.
            if (beatmap.ControlPointInfo.Groups.Any(g => !double.IsFinite(g.Time + offset) || g.Time + offset < int.MinValue || g.Time + offset > int.MaxValue))
                return "This shift would move a timing point beyond the supported time range. Nothing was moved.";

            return null;
        }

        public static bool Apply(EditorBeatmap beatmap, int offset, out string? error)
        {
            error = Validate(beatmap, offset);
            if (error != null || offset == 0)
                return false;

            var groups = beatmap.ControlPointInfo.Groups.Select(g => (Group: g, Points: g.ControlPoints.ToArray())).ToArray();
            var objects = beatmap.HitObjects.ToArray();
            var breaks = beatmap.Breaks.Select(b => b is ManualBreakPeriod
                ? new ManualBreakPeriod(b.StartTime + offset, b.EndTime + offset)
                : new BreakPeriod(b.StartTime + offset, b.EndTime + offset)).ToArray();
            var bookmarks = beatmap.Bookmarks.Select(t => t + offset).ToArray();

            beatmap.BeginChange();
            try
            {
                // Keep the ControlPointInfo instance (UI observers bind to it), but detach all groups
                // before inserting any: a translated group may occupy another group's old timestamp.
                foreach (var group in groups)
                    beatmap.ControlPointInfo.RemoveGroup(group.Group);

                foreach (var group in groups)
                {
                    var shiftedGroup = beatmap.ControlPointInfo.GroupAt(group.Group.Time + offset, true);
                    foreach (var point in group.Points)
                        shiftedGroup.Add(point);
                }

                foreach (var hitObject in objects)
                    hitObject.StartTime += offset;

                beatmap.Breaks.Clear();
                beatmap.Breaks.AddRange(breaks);
                beatmap.Bookmarks.Clear();
                beatmap.Bookmarks.AddRange(bookmarks);

                if (beatmap.PreviewTime.Value >= 0)
                    beatmap.PreviewTime.Value += offset;

                // Apply defaults only after BOTH timing and objects have moved, preserving slider SV/duration.
                beatmap.UpdateAllHitObjects();
            }
            finally
            {
                beatmap.EndChange();
            }

            return true;
        }

        private static bool validTime(double time) => double.IsFinite(time) && time >= 0 && time <= int.MaxValue;
    }
}
