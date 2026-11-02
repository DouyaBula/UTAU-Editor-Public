// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Screens.Edit
{
    internal enum EditorHitsoundCopyMode
    {
        OverwriteEverything,
        OverwriteOnlyDefined,
    }

    internal readonly struct EditorHitsoundCopyResult
    {
        public readonly int MatchedObjects;
        public readonly int MatchedNodes;
        public readonly int UpdatedObjects;

        public EditorHitsoundCopyResult(int matchedObjects, int matchedNodes, int updatedObjects)
        {
            MatchedObjects = matchedObjects;
            MatchedNodes = matchedNodes;
            UpdatedObjects = updatedObjects;
        }
    }

    internal static class EditorHitsoundCopier
    {
        public static EditorHitsoundCopyResult Copy(IBeatmap sourceBeatmap, EditorBeatmap targetBeatmap, double temporalLeniency, EditorHitsoundCopyMode mode)
        {
            HitsoundTimeMatcher.ValidateTolerance(temporalLeniency);
            var sourceEvents = sourceBeatmap.HitObjects
                                            .OrderBy(h => h.StartTime)
                                            .SelectMany(h => createSourceEvents(h, mode))
                                            .OrderBy(e => e.Time)
                                            .ToArray();
            var targetObjects = targetBeatmap.HitObjects.OrderBy(h => h.StartTime).ToArray();
            var targetSlots = targetObjects.SelectMany(createTargetSlots).OrderBy(s => s.Time).ToArray();
            var usedTargetSlotIndices = new HashSet<int>();
            var updatedTargetObjects = new HashSet<HitObject>();
            int matchedObjects = 0;
            int matchedNodes = 0;

            if (mode == EditorHitsoundCopyMode.OverwriteEverything)
            {
                foreach (var target in targetObjects)
                {
                    resetSamples(target);
                    updatedTargetObjects.Add(target);
                }
            }

            foreach (var sourceEvent in sourceEvents)
            {
                int targetSlotIndex = findNearestUnusedTargetSlot(sourceEvent.Time, targetSlots, usedTargetSlotIndices, temporalLeniency);

                if (targetSlotIndex < 0)
                    continue;

                var targetSlot = targetSlots[targetSlotIndex];
                targetSlot.SetSamples(cloneStableSamples(sourceEvent.Samples));
                usedTargetSlotIndices.Add(targetSlotIndex);
                updatedTargetObjects.Add(targetSlot.HitObject);

                if (targetSlot.IsSliderRepeatOrTail)
                    matchedNodes++;
                else
                    matchedObjects++;
            }

            foreach (var target in updatedTargetObjects)
                targetBeatmap.Update(target);

            return new EditorHitsoundCopyResult(matchedObjects, matchedNodes, updatedTargetObjects.Count);
        }

        private static IEnumerable<SourceSampleEvent> createSourceEvents(HitObject hitObject, EditorHitsoundCopyMode mode)
        {
            if (hitObject is IHasRepeats repeats)
            {
                int nodeCount = repeats.RepeatCount + 2;

                for (int i = 0; i < nodeCount; i++)
                {
                    var samples = getSourceSliderNodeSamples(hitObject, repeats, i, mode);

                    if (mode == EditorHitsoundCopyMode.OverwriteOnlyDefined && !hasDefinedSamples(samples))
                        continue;

                    yield return new SourceSampleEvent(getSliderNodeTime(hitObject, repeats, i), samples);
                }
            }
            else
            {
                if (mode == EditorHitsoundCopyMode.OverwriteEverything || hasDefinedSamples(hitObject.Samples))
                    yield return new SourceSampleEvent(hitObject.StartTime, hitObject.Samples);
            }
        }

        private static IList<HitSampleInfo> getSourceSliderNodeSamples(HitObject hitObject, IHasRepeats repeats, int nodeIndex, EditorHitsoundCopyMode mode)
        {
            IList<HitSampleInfo>? nodeSamples = nodeIndex < repeats.NodeSamples.Count ? repeats.NodeSamples[nodeIndex] : null;

            if (nodeIndex == 0)
            {
                if (mode == EditorHitsoundCopyMode.OverwriteEverything)
                {
                    if (nodeSamples != null && nodeSamples.Count > 0)
                        return nodeSamples;

                    return hitObject.Samples;
                }

                return nodeSamples != null && hasDefinedSamples(nodeSamples) ? nodeSamples : hitObject.Samples;
            }

            return nodeSamples ?? Array.Empty<HitSampleInfo>();
        }

        private static IEnumerable<TargetSampleSlot> createTargetSlots(HitObject hitObject)
        {
            if (hitObject is IHasRepeats repeats)
            {
                int nodeCount = repeats.RepeatCount + 2;

                for (int i = 0; i < nodeCount; i++)
                    yield return new TargetSampleSlot(hitObject, getSliderNodeTime(hitObject, repeats, i), i);
            }
            else
                yield return new TargetSampleSlot(hitObject, hitObject.StartTime, -1);
        }

        private static double getSliderNodeTime(HitObject hitObject, IHasRepeats repeats, int nodeIndex)
        {
            double progress = nodeIndex / (double)repeats.SpanCount();
            return hitObject.StartTime + repeats.Duration * progress;
        }

        private static int findNearestUnusedTargetSlot(double sourceTime, IReadOnlyList<TargetSampleSlot> targetSlots, HashSet<int> usedTargetSlotIndices, double temporalLeniency)
            => HitsoundTimeMatcher.FindNearest(targetSlots, sourceTime, temporalLeniency, slot => slot.Time, i => !usedTargetSlotIndices.Contains(i));

        private static bool hasDefinedSamples(IList<HitSampleInfo> samples) => samples.Any(s => !isDefaultNormalSample(s));

        private static bool isDefaultNormalSample(HitSampleInfo sample) =>
            sample.Name == HitSampleInfo.HIT_NORMAL
            && sample.Bank == SampleControlPoint.DEFAULT_BANK
            && sample.Suffix == null
            && !sample.UseBeatmapSamples
            && (sample.Volume == 0 || sample.Volume == 100);

        private static void resetSamples(HitObject hitObject)
        {
            hitObject.Samples = Array.Empty<HitSampleInfo>();

            if (hitObject is IHasRepeats repeats)
            {
                ensureNodeSampleCount(repeats);

                for (int i = 0; i < repeats.NodeSamples.Count; i++)
                    repeats.NodeSamples[i] = new List<HitSampleInfo>();
            }
        }

        private static void ensureNodeSampleCount(IHasRepeats repeats)
        {
            while (repeats.NodeSamples.Count < repeats.RepeatCount + 2)
                repeats.NodeSamples.Add(new List<HitSampleInfo>());
        }

        private static List<HitSampleInfo> cloneStableSamples(IList<HitSampleInfo> samples)
        {
            if (samples.Count == 0)
                return new List<HitSampleInfo>();

            var builtInSamples = new Dictionary<string, HitSampleInfo>();
            var customSamples = new List<HitSampleInfo>();
            string? additionBank = null;

            foreach (var sample in samples)
            {
                if (!isLegacyBuiltInSample(sample.Name))
                {
                    customSamples.Add(sample.With());
                    continue;
                }

                builtInSamples[sample.Name] = sample;

                if (additionBank == null && isLegacyAdditionSample(sample.Name))
                    additionBank = normaliseBank(sample.Bank);
            }

            if (builtInSamples.Count == 0)
                return customSamples;

            string normalBank = SampleControlPoint.DEFAULT_BANK;
            int volume = 100;

            if (builtInSamples.TryGetValue(HitSampleInfo.HIT_NORMAL, out var normalSample))
            {
                normalBank = normaliseBank(normalSample.Bank);
                volume = normalSample.Volume;
            }
            else
            {
                var firstBuiltInSample = builtInSamples.Values.First();
                volume = firstBuiltInSample.Volume;
                builtInSamples[HitSampleInfo.HIT_NORMAL] = new HitSampleInfo(HitSampleInfo.HIT_NORMAL, normalBank, volume: volume, editorAutoBank: false);
            }

            var result = new List<HitSampleInfo>
            {
                createStableSample(builtInSamples[HitSampleInfo.HIT_NORMAL], normalBank, volume),
            };

            additionBank ??= normalBank;

            addStableAdditionIfPresent(HitSampleInfo.HIT_WHISTLE);
            addStableAdditionIfPresent(HitSampleInfo.HIT_FINISH);
            addStableAdditionIfPresent(HitSampleInfo.HIT_CLAP);
            result.AddRange(customSamples);

            return result;

            void addStableAdditionIfPresent(string name)
            {
                if (builtInSamples.TryGetValue(name, out var sample))
                    result.Add(createStableSample(sample, additionBank, volume));
            }
        }

        private static string normaliseBank(string? bank)
        {
            bank = bank?.Trim().ToLowerInvariant();

            return bank switch
            {
                HitSampleInfo.BANK_SOFT => HitSampleInfo.BANK_SOFT,
                HitSampleInfo.BANK_DRUM => HitSampleInfo.BANK_DRUM,
                _ => SampleControlPoint.DEFAULT_BANK,
            };
        }

        private static HitSampleInfo createStableSample(HitSampleInfo sample, string bank, int volume)
            => new HitSampleInfo(sample.Name, bank, sample.Suffix, volume, false);

        private static bool isLegacyBuiltInSample(string name)
            => name is HitSampleInfo.HIT_NORMAL or HitSampleInfo.HIT_WHISTLE or HitSampleInfo.HIT_FINISH or HitSampleInfo.HIT_CLAP;

        private static bool isLegacyAdditionSample(string name)
            => name is HitSampleInfo.HIT_WHISTLE or HitSampleInfo.HIT_FINISH or HitSampleInfo.HIT_CLAP;

        private readonly struct SourceSampleEvent
        {
            public readonly double Time;
            public readonly IList<HitSampleInfo> Samples;

            public SourceSampleEvent(double time, IList<HitSampleInfo> samples)
            {
                Time = time;
                Samples = samples;
            }
        }

        private readonly struct TargetSampleSlot
        {
            public readonly HitObject HitObject;
            public readonly double Time;

            private readonly int sliderNodeIndex;

            public bool IsSliderRepeatOrTail => sliderNodeIndex > 0;

            public TargetSampleSlot(HitObject hitObject, double time, int sliderNodeIndex)
            {
                HitObject = hitObject;
                Time = time;
                this.sliderNodeIndex = sliderNodeIndex;
            }

            public void SetSamples(List<HitSampleInfo> samples)
            {
                if (sliderNodeIndex < 0)
                {
                    HitObject.Samples = samples;
                    return;
                }

                var repeats = (IHasRepeats)HitObject;
                ensureNodeSampleCount(repeats);
                repeats.NodeSamples[sliderNodeIndex] = samples;
            }
        }
    }
}
