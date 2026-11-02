// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;

namespace osu.Game.Screens.Edit
{
    /// <summary>Matches only the relevant window of time-sorted targets, keeping earliest ties.</summary>
    internal static class HitsoundTimeMatcher
    {
        public static void ValidateTolerance(double tolerance)
        {
            if (!double.IsFinite(tolerance) || tolerance < 0)
                throw new ArgumentOutOfRangeException(nameof(tolerance), "Matching tolerance must be finite and non-negative.");
        }

        public static int FindNearest<T>(IReadOnlyList<T> targets, double time, double tolerance, Func<T, double> getTime,
                                         Func<int, bool>? eligible = null, Func<T, bool>? preferred = null)
        {
            ValidateTolerance(tolerance);
            if (!double.IsFinite(time))
                return -1;

            int low = 0;
            int high = targets.Count;
            while (low < high)
            {
                int middle = low + (high - low) / 2;
                if (getTime(targets[middle]) < time - tolerance)
                    low = middle + 1;
                else
                    high = middle;
            }

            int best = -1;
            double bestDistance = double.PositiveInfinity;
            bool bestPreferred = false;
            for (int i = low; i < targets.Count; i++)
            {
                double targetTime = getTime(targets[i]);
                if (targetTime > time + tolerance)
                    break;
                if (eligible != null && !eligible(i))
                    continue;

                double distance = Math.Abs(targetTime - time);
                bool isPreferred = preferred?.Invoke(targets[i]) == true;
                if (bestPreferred && !isPreferred || isPreferred == bestPreferred && distance >= bestDistance)
                    continue;

                best = i;
                bestDistance = distance;
                bestPreferred = isPreferred;
            }

            return best;
        }
    }
}
