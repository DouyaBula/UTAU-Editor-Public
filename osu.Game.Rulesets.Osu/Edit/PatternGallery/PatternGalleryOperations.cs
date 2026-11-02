// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Logging;
using osu.Game.Beatmaps.Timing;
using osu.Game.IO.Serialization;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit;

namespace osu.Game.Rulesets.Osu.Edit.PatternGallery
{
    /// <summary>Pure capture and paste transformations used by the gallery UI.</summary>
    public static class PatternGalleryOperations
    {
        public static PatternGalleryEntry Capture(IEnumerable<HitObject> hitObjects, string name, string tags = "", EditorBeatmap? sourceBeatmap = null)
        {
            var objects = hitObjects.OrderBy(h => h.StartTime).ToList();
            if (objects.Count == 0)
                throw new ArgumentException("At least one hit object must be selected.", nameof(hitObjects));

            double start = objects.Min(h => h.StartTime);
            double end = objects.Max(h => h.GetEndTime());
            var now = DateTime.UtcNow;

            var entry = new PatternGalleryEntry
            {
                Name = string.IsNullOrWhiteSpace(name) ? "Unnamed pattern" : name.Trim(),
                Tags = tags?.Trim() ?? string.Empty,
                CreatedUtc = now,
                LastUsedUtc = now,
                ObjectCount = objects.Count,
                Duration = Math.Max(0, end - start),
                SerializedObjects = new ClipboardContent { HitObjects = objects }.Serialize()
            };

            if (sourceBeatmap != null)
            {
                entry.SourceBeatLength = getFullBeatLengthAtTime(sourceBeatmap, start);
                entry.RelativeStartBeats = objects.Select(hitObject => BeatTimeline.BeatsBetween(sourceBeatmap.ControlPointInfo, start, hitObject.StartTime)).ToList();
                entry.DurationBeats = objects.Select(hitObject => BeatTimeline.BeatsBetween(sourceBeatmap.ControlPointInfo, hitObject.StartTime, hitObject.GetEndTime())).ToList();
            }

            return entry;
        }

        public static List<HitObject> Deserialize(PatternGalleryEntry entry)
        {
            if (string.IsNullOrWhiteSpace(entry.SerializedObjects))
                return new List<HitObject>();

            try
            {
                var content = entry.SerializedObjects.Deserialize<ClipboardContent>();
                return content?.HitObjects?.Where(h => h != null).ToList() ?? new List<HitObject>();
            }
            catch (Exception e)
            {
                // A single stale/hand-edited entry must not make the entire gallery unusable.
                // The caller can surface the empty result as a per-entry error and remove it.
                Logger.Error(e, $"Failed to deserialize pattern gallery entry '{entry.Name}'.");
                return new List<HitObject>();
            }
        }

        /// <summary>
        /// Returns fresh hit objects with their relative timing shifted so the pattern starts at
        /// <paramref name="targetTime"/>. No source entry or stored payload is mutated.
        /// </summary>
        public static List<HitObject> PrepareForInsertion(PatternGalleryEntry entry, double targetTime, EditorBeatmap? destinationBeatmap = null)
        {
            var objects = Deserialize(entry);
            if (objects.Count == 0)
                return objects;

            if (destinationBeatmap != null && entry.HasTimingMetadata && objects.Count == entry.ObjectCount)
                return prepareWithBeatTiming(entry, objects, targetTime, destinationBeatmap);

            double offset = targetTime - objects.Min(h => h.StartTime);
            foreach (var hitObject in objects)
                hitObject.StartTime += offset;

            return objects;
        }

        private static List<HitObject> prepareWithBeatTiming(PatternGalleryEntry entry, List<HitObject> objects, double targetTime, EditorBeatmap beatmap)
        {
            var orderedObjects = objects.OrderBy(hitObject => hitObject.StartTime).ToList();

            for (int i = 0; i < orderedObjects.Count; i++)
            {
                var hitObject = orderedObjects[i];
                hitObject.StartTime = BeatTimeline.TimeAfterBeats(beatmap.ControlPointInfo, targetTime, entry.RelativeStartBeats[i]);

                double desiredDuration = Math.Max(0, BeatTimeline.TimeAfterBeats(beatmap.ControlPointInfo, hitObject.StartTime, entry.DurationBeats[i]) - hitObject.StartTime);

                switch (hitObject)
                {
                    case Slider slider when desiredDuration > 0:
                        // Apply destination defaults before calculating the velocity correction.
                        // This keeps the path and repeats intact while changing only the SV needed
                        // to occupy the same number of destination beats as the source slider.
                        slider.ApplyDefaults(beatmap.ControlPointInfo, beatmap.Difficulty);
                        double nativeDuration = slider.GetEndTime() - slider.StartTime;

                        if (nativeDuration > 0)
                        {
                            setVelocity(slider, slider.SliderVelocityMultiplier * nativeDuration / desiredDuration);
                            slider.UpdateVelocity(beatmap.ControlPointInfo, beatmap.Difficulty);
                        }

                        break;

                    case IHasDuration duration:
                        duration.Duration = desiredDuration;
                        break;
                }
            }

            return orderedObjects;
        }

        private static double getFullBeatLengthAtTime(EditorBeatmap beatmap, double time)
        {
            double beatLength = beatmap.ControlPointInfo.TimingPointAt(time).BeatLength;
            return beatLength > 0 ? beatLength : 500;
        }

        private static void setVelocity(Slider slider, double velocity)
        {
            if (!double.IsFinite(velocity) || velocity < slider.SliderVelocityMultiplierBindable.MinValue - 1e-9
                                           || velocity > slider.SliderVelocityMultiplierBindable.MaxValue + 1e-9)
                throw new InvalidOperationException($"Preserving the pattern's shape and rhythm requires {velocity:0.###}x SV at {slider.StartTime:0} ms. Insertion cancelled; adjust the destination slider multiplier or disable beat timing.");
            slider.SliderVelocityMultiplier = Math.Clamp(velocity, slider.SliderVelocityMultiplierBindable.MinValue, slider.SliderVelocityMultiplierBindable.MaxValue);
        }

        /// <summary>
        /// Snaps every inserted object's start time to the destination beatmap. Duration-bearing
        /// objects are snapped at both ends; sliders keep their snapped tail by compensating the
        /// native slider velocity multiplier. This is deliberately performed on the deserialised
        /// copies before they enter the beatmap, so the source pattern is never changed.
        /// </summary>
        public static void SnapToDestinationTiming(IList<HitObject> objects, EditorBeatmap beatmap)
        {
            foreach (var hitObject in objects.OrderBy(h => h.StartTime))
            {
                double oldStart = hitObject.StartTime;
                double oldEnd = hitObject.GetEndTime();
                double newStart = beatmap.SnapTime(oldStart, null);
                double newEnd = Math.Max(newStart + beatmap.GetBeatLengthAtTime(newStart), beatmap.SnapTime(oldEnd, newStart));

                hitObject.StartTime = newStart;

                switch (hitObject)
                {
                    // Slider duration is derived from path distance, repeats, destination timing
                    // and velocity. Apply destination defaults once before calculating the ratio;
                    // otherwise a source map with a different BPM would skew the resulting tail.
                    case IHasPathWithRepeats and IHasSliderVelocity sliderVelocity:
                    {
                        double newDuration = newEnd - newStart;

                        if (newDuration > 0 && hitObject is Slider slider)
                        {
                            slider.ApplyDefaults(beatmap.ControlPointInfo, beatmap.Difficulty);
                            double destinationDuration = slider.GetEndTime() - newStart;
                            double adjustedVelocity = sliderVelocity.SliderVelocityMultiplier * destinationDuration / newDuration;
                            setVelocity(slider, adjustedVelocity);
                            slider.UpdateVelocity(beatmap.ControlPointInfo, beatmap.Difficulty);
                        }

                        break;
                    }

                    case IHasDuration duration:
                        duration.Duration = newEnd - newStart;
                        break;
                }
            }
        }
    }
}
