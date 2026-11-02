// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Beatmaps.Timing
{
    /// <summary>
    /// Beat-domain arithmetic across BPM changes, shared by pattern insertion and generators.
    /// A red line changes the beat length, not the amount of elapsed musical time.
    /// </summary>
    public static class BeatTimeline
    {
        public static double BeatsBetween(ControlPointInfo timing, double start, double end)
        {
            requireFinite(start);
            requireFinite(end);
            if (end < start)
                return -BeatsBetween(timing, end, start);

            double beats = 0;
            while (start < end)
            {
                double next = Math.Min(end, timing.TimingPointAfter(start)?.Time ?? end);
                beats += (next - start) / beatLength(timing, start);
                start = next;
            }

            return beats;
        }

        public static double TimeAfterBeats(ControlPointInfo timing, double start, double beats)
        {
            requireFinite(start);
            requireFinite(beats);
            if (beats < 0)
                throw new ArgumentOutOfRangeException(nameof(beats));

            while (beats > 0)
            {
                double length = beatLength(timing, start);
                double next = timing.TimingPointAfter(start)?.Time ?? double.PositiveInfinity;
                double available = (next - start) / length;
                if (beats <= available)
                {
                    double result = start + beats * length;
                    requireFinite(result);
                    return result;
                }

                beats -= available;
                start = next;
            }

            return start;
        }

        private static double beatLength(ControlPointInfo timing, double time)
        {
            double length = timing.TimingPointAt(time).BeatLength;
            if (!double.IsFinite(length) || length <= 0)
                throw new ArgumentException("Timing must contain finite, positive beat lengths.");
            return length;
        }

        private static void requireFinite(double value)
        {
            if (!double.IsFinite(value))
                throw new ArgumentOutOfRangeException(nameof(value), "Time and beat counts must be finite.");
        }
    }
}
