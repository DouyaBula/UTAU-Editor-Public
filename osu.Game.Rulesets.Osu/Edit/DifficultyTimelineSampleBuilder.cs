// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Screens.Edit.Compose.Components.Timeline;

namespace osu.Game.Rulesets.Osu.Edit
{
    /// <summary>
    /// Combines independently-scaled skill strain intervals into a single local difficulty profile.
    /// </summary>
    internal static class DifficultyTimelineSampleBuilder
    {
        private const int max_sample_points = 1500;

        public static IReadOnlyList<DifficultyRatingSample> Build(IReadOnlyList<DifficultyTimelineInterval> intervals, double maxTime, double step)
        {
            if (intervals.Count == 0)
                return Array.Empty<DifficultyRatingSample>();

            if (!double.IsFinite(step) || step <= 0)
                step = 400;

            // Match MapsetVerifier's 400ms base buckets (or the user-configured interval),
            // widening the grid in 50ms increments on long maps to keep its chart payload bounded.
            double mapDuration = Math.Max(0, maxTime);
            int pointCountAtConfiguredStep = (int)Math.Ceiling(mapDuration / step) + 1;

            if (pointCountAtConfiguredStep > max_sample_points)
            {
                double requiredStep = Math.Ceiling(mapDuration / (max_sample_points - 1));
                step = Math.Max(step, Math.Ceiling(requiredStep / 50) * 50);
            }

            int sampleCount = Math.Clamp((int)Math.Ceiling(mapDuration / step) + 1, 1, max_sample_points);
            var skillIntervals = intervals.GroupBy(interval => interval.SkillId).ToArray();
            var rawWeights = skillIntervals.ToDictionary(group => group.Key, group =>
            {
                double weight = group.First().SkillWeight;
                return double.IsFinite(weight) ? Math.Max(0, weight) : 0;
            });

            double totalWeight = rawWeights.Values.Sum();
            var weights = rawWeights.ToDictionary(pair => pair.Key, pair => totalWeight > 0 ? pair.Value / totalWeight : 1d / skillIntervals.Length);
            var names = skillIntervals.ToDictionary(group => group.Key, group => group.First().SkillName);
            var valuesBySkill = new Dictionary<int, double[]>(skillIntervals.Length);
            var peaks = new Dictionary<int, double>(skillIntervals.Length);

            foreach (var group in skillIntervals)
            {
                var orderedIntervals = group.OrderBy(interval => interval.StartTime).ToArray();
                var skillSamples = new double[sampleCount];
                int intervalIndex = 0;

                for (int sampleIndex = 0; sampleIndex < sampleCount; sampleIndex++)
                {
                    double bucketStart = sampleIndex * step;
                    double bucketEnd = bucketStart + step;

                    // Resample the highest local strain in [t, t + step), preserving short peaks
                    // while allowing explicit zero intervals (breaks) to fall back to zero.
                    while (intervalIndex < orderedIntervals.Length && orderedIntervals[intervalIndex].EndTime <= bucketStart)
                        intervalIndex++;

                    double bucketPeak = 0;

                    for (int scanIndex = intervalIndex;
                         scanIndex < orderedIntervals.Length && orderedIntervals[scanIndex].StartTime < bucketEnd;
                         scanIndex++)
                    {
                        if (orderedIntervals[scanIndex].EndTime > bucketStart)
                            bucketPeak = Math.Max(bucketPeak, orderedIntervals[scanIndex].Value);
                    }

                    skillSamples[sampleIndex] = bucketPeak;
                }

                valuesBySkill.Add(group.Key, skillSamples);
                peaks.Add(group.Key, skillSamples.Max());
            }

            var samples = new List<DifficultyRatingSample>(sampleCount);

            for (int i = 0; i < sampleCount; i++)
            {
                double totalDifficulty = 0;
                double dominantContribution = 0;
                string dominantSkill = string.Empty;

                foreach (var (skillId, skillValues) in valuesBySkill)
                {
                    double skillPeak = peaks[skillId];
                    double contribution = skillPeak > 0
                        ? Math.Clamp(skillValues[i] / skillPeak, 0, 1) * weights[skillId]
                        : 0;
                    totalDifficulty += contribution;

                    if (contribution > dominantContribution)
                    {
                        dominantContribution = contribution;
                        dominantSkill = names[skillId];
                    }
                }

                samples.Add(new DifficultyRatingSample(i * step, Math.Clamp(totalDifficulty, 0, 1), dominantSkill));
            }

            return samples;
        }
    }
}
