// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Screens.Edit
{
    public readonly struct EditorHitsoundDifficultyExportResult
    {
        public readonly string DifficultyName;
        public readonly int NoteCount;

        public EditorHitsoundDifficultyExportResult(string difficultyName, int noteCount)
        {
            DifficultyName = difficultyName;
            NoteCount = noteCount;
        }
    }

    public readonly struct EditorHitsoundManiaMergeResult
    {
        public readonly int SourceNotes;
        public readonly int MatchedNotes;
        public readonly int UpdatedObjects;

        public EditorHitsoundManiaMergeResult(int sourceNotes, int matchedNotes, int updatedObjects)
        {
            SourceNotes = sourceNotes;
            MatchedNotes = matchedNotes;
            UpdatedObjects = updatedObjects;
        }
    }

    public static class EditorHitsoundDifficultyTools
    {
        public const int DEFAULT_MANIA_COLUMNS = 4;

        public const string DEFAULT_MANIA_COLUMN_SAMPLE_MAPPING =
            "1=normal-hitnormal;2=normal-hitwhistle;3=normal-hitfinish;4=normal-hitclap;5=soft-hitwhistle;6=soft-hitfinish;7=soft-hitclap;8=drum-hitwhistle;9=drum-hitfinish;10=drum-hitclap";

        public const string DEFAULT_MANIA_DEFAULT_NORMAL_SAMPLE_BANK = HitSampleInfo.BANK_NORMAL;

        public static EditorHitsoundDifficultyExportResult ExportStandardNotesDifficulty(EditorBeatmap editorBeatmap, BeatmapManager beatmapManager, bool includeSliderEnds)
        {
            var beatmap = createNewDifficultyBeatmap(editorBeatmap, beatmapManager, editorBeatmap.BeatmapInfo.Ruleset, $"{editorBeatmap.BeatmapInfo.DifficultyName} (hitsound notes)");

            beatmap.HitObjects.Clear();
            beatmap.HitObjects.AddRange(createReferenceObjects(editorBeatmap, includeSliderEnds, timePosition => new ReferenceHitObject
            {
                StartTime = timePosition.Time,
                Position = timePosition.Position,
            }));

            saveDifficulty(beatmapManager, beatmap);

            return new EditorHitsoundDifficultyExportResult(beatmap.BeatmapInfo.DifficultyName, beatmap.HitObjects.Count);
        }

        public static EditorHitsoundDifficultyExportResult ExportManiaReferenceDifficulty(EditorBeatmap editorBeatmap, BeatmapManager beatmapManager, RulesetInfo maniaRuleset, bool includeSliderEnds, int columns)
        {
            columns = Math.Clamp(columns, 1, 10);

            var beatmap = createNewDifficultyBeatmap(editorBeatmap, beatmapManager, maniaRuleset, $"{editorBeatmap.BeatmapInfo.DifficultyName} (mania hitsound reference)");
            beatmap.Difficulty.CircleSize = columns;
            beatmap.HitObjects.Clear();
            beatmap.HitObjects.AddRange(createReferenceObjects(editorBeatmap, includeSliderEnds, timePosition => new ReferenceHitObject
            {
                StartTime = timePosition.Time,
                Position = new Vector2(0, timePosition.Position.Y),
            }));

            saveDifficulty(beatmapManager, beatmap);

            return new EditorHitsoundDifficultyExportResult(beatmap.BeatmapInfo.DifficultyName, beatmap.HitObjects.Count);
        }

        public static EditorHitsoundManiaMergeResult MergeManiaHitsoundsInto(EditorBeatmap targetBeatmap, BeatmapManager beatmapManager, BeatmapInfo maniaBeatmapInfo, string columnSampleMapping, double temporalLeniency,
                                                                             bool clearExistingHitsounds)
            => MergeManiaHitsoundsInto(targetBeatmap, beatmapManager, maniaBeatmapInfo, columnSampleMapping, DEFAULT_MANIA_DEFAULT_NORMAL_SAMPLE_BANK, temporalLeniency, clearExistingHitsounds);

        public static EditorHitsoundManiaMergeResult MergeManiaHitsoundsInto(EditorBeatmap targetBeatmap, BeatmapManager beatmapManager, BeatmapInfo maniaBeatmapInfo, string columnSampleMapping,
                                                                             string defaultNormalSampleBank, double temporalLeniency, bool clearExistingHitsounds)
        {
            HitsoundTimeMatcher.ValidateTolerance(temporalLeniency);
            string fallbackNormalBank = normaliseBank(defaultNormalSampleBank);
            var maniaWorkingBeatmap = beatmapManager.GetWorkingBeatmap(maniaBeatmapInfo, true);
            var maniaBeatmap = maniaWorkingBeatmap.Beatmap;
            int columns = Math.Clamp((int)Math.Round(maniaBeatmap.Difficulty.CircleSize), 1, 10);
            var mappings = parseColumnSampleMapping(columnSampleMapping);
            var notes = maniaBeatmap.HitObjects
                                    .OrderBy(h => h.StartTime)
                                    .Select(h => new ManiaReferenceNote(h.StartTime, getColumn(h, columns)))
                                    .ToArray();
            var targets = createTargets(targetBeatmap).OrderBy(t => t.Time).ToArray();
            var updatedObjects = new HashSet<HitObject>();
            int matched = 0;

            if (clearExistingHitsounds)
            {
                foreach (var hitObject in targetBeatmap.HitObjects)
                {
                    resetSamples(hitObject);
                    updatedObjects.Add(hitObject);
                }
            }

            foreach (var note in notes)
            {
                if (!mappings.TryGetValue(note.Column, out var samples) || samples.Count == 0)
                    continue;

                var target = findNearestTarget(note.Time, targets, temporalLeniency);

                if (target == null)
                    continue;

                foreach (var sample in samples)
                    target.AddSample(sample.With());

                updatedObjects.Add(target.HitObject);
                matched++;
            }

            foreach (var hitObject in updatedObjects)
            {
                normaliseSamplesForStable(hitObject, fallbackNormalBank);
                targetBeatmap.Update(hitObject);
            }

            return new EditorHitsoundManiaMergeResult(notes.Length, matched, updatedObjects.Count);
        }

        private static Beatmap createNewDifficultyBeatmap(EditorBeatmap editorBeatmap, BeatmapManager beatmapManager, RulesetInfo ruleset, string requestedDifficultyName)
        {
            var targetSet = beatmapManager.QueryBeatmap(b => b.ID == editorBeatmap.BeatmapInfo.ID).AsNonNull().BeatmapSet.AsNonNull();
            var referenceWorkingBeatmap = beatmapManager.GetWorkingBeatmap(editorBeatmap.BeatmapInfo, true);
            var newWorkingBeatmap = beatmapManager.CreateNewDifficulty(targetSet, referenceWorkingBeatmap, ruleset);

            if (newWorkingBeatmap.Beatmap is not Beatmap beatmap)
                throw new InvalidOperationException("Created difficulty is not backed by a mutable beatmap.");

            beatmap.BeatmapInfo.DifficultyName = NamingUtils.GetNextBestName(targetSet.Beatmaps.Select(b => b.DifficultyName), requestedDifficultyName);
            beatmap.Difficulty = editorBeatmap.Difficulty.Clone();
            beatmap.ControlPointInfo = editorBeatmap.ControlPointInfo.DeepClone();
            beatmap.Breaks = new osu.Framework.Lists.SortedList<BreakPeriod>();

            foreach (var breakPeriod in editorBeatmap.Breaks)
                beatmap.Breaks.Add(breakPeriod);

            beatmap.Bookmarks = editorBeatmap.Bookmarks.ToArray();
            beatmap.AudioLeadIn = editorBeatmap.AudioLeadIn;
            beatmap.StackLeniency = editorBeatmap.StackLeniency;
            beatmap.SpecialStyle = editorBeatmap.SpecialStyle;
            beatmap.LetterboxInBreaks = editorBeatmap.LetterboxInBreaks;
            beatmap.WidescreenStoryboard = editorBeatmap.WidescreenStoryboard;
            beatmap.EpilepsyWarning = editorBeatmap.EpilepsyWarning;
            beatmap.SamplesMatchPlaybackRate = editorBeatmap.SamplesMatchPlaybackRate;
            beatmap.DistanceSpacing = editorBeatmap.DistanceSpacing;
            beatmap.GridSize = editorBeatmap.GridSize;
            beatmap.TimelineZoom = editorBeatmap.TimelineZoom;
            beatmap.Countdown = editorBeatmap.Countdown;
            beatmap.CountdownOffset = editorBeatmap.CountdownOffset;

            return beatmap;
        }

        private static void saveDifficulty(BeatmapManager beatmapManager, Beatmap beatmap)
        {
            beatmap.BeatmapInfo.Hash = string.Empty;
            beatmap.BeatmapInfo.ResetOnlineInfo();
            beatmapManager.Save(beatmap.BeatmapInfo, beatmap);
        }

        private static IEnumerable<HitObject> createReferenceObjects(EditorBeatmap editorBeatmap, bool includeSliderEnds, Func<TimePosition, HitObject> createObject)
        {
            foreach (var hitObject in editorBeatmap.HitObjects.OrderBy(h => h.StartTime))
            {
                if (hitObject is not IHasPosition hasPosition)
                    continue;

                yield return createObject(new TimePosition(hitObject.StartTime, hasPosition.Position));

                if (includeSliderEnds && hitObject is IHasRepeats repeats)
                    yield return createObject(new TimePosition(hitObject.StartTime + repeats.Duration, getEndPosition(hitObject)));
            }
        }

        private static Vector2 getEndPosition(HitObject hitObject)
        {
            if (hitObject is IHasPathWithRepeats pathWithRepeats && hitObject is IHasPosition pathPosition)
                return pathPosition.Position + pathWithRepeats.CurvePositionAt(1);

            if (hitObject is IHasPosition hasPosition)
                return hasPosition.Position;

            return new Vector2(256, 192);
        }

        private static int getColumn(HitObject hitObject, int columns)
        {
            if (hitObject is not IHasXPosition hasXPosition)
                return 0;

            return Math.Clamp((int)Math.Floor(hasXPosition.X / (512f / columns)), 0, columns - 1);
        }

        private static IReadOnlyDictionary<int, IReadOnlyList<HitSampleInfo>> parseColumnSampleMapping(string mapping)
        {
            var result = new Dictionary<int, IReadOnlyList<HitSampleInfo>>();

            foreach (string entry in mapping.Split([';', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                string[] pair = entry.Split('=', 2, StringSplitOptions.TrimEntries);

                if (pair.Length != 2 || !int.TryParse(pair[0], out int oneBasedColumn))
                    continue;

                var samples = pair[1].Split(['+', '|'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                     .Select(parseSample)
                                     .Where(s => s != null)
                                     .Cast<HitSampleInfo>()
                                     .ToArray();

                result[Math.Max(0, oneBasedColumn - 1)] = samples;
            }

            return result;
        }

        private static HitSampleInfo? parseSample(string text)
        {
            string[] parts = text.Split('-', 2, StringSplitOptions.TrimEntries);
            string bank = HitSampleInfo.BANK_NORMAL;
            string name;

            if (parts.Length == 2)
            {
                bank = normaliseBank(parts[0]);
                name = normaliseSampleName(parts[1]);
            }
            else
                name = normaliseSampleName(parts[0]);

            if (string.IsNullOrEmpty(name))
                return null;

            return new HitSampleInfo(name, bank, volume: 100, editorAutoBank: false);
        }

        private static string normaliseBank(string text)
        {
            text = text.Trim().ToLowerInvariant();

            return text switch
            {
                "soft" => HitSampleInfo.BANK_SOFT,
                "drum" => HitSampleInfo.BANK_DRUM,
                _ => HitSampleInfo.BANK_NORMAL,
            };
        }

        private static string normaliseSampleName(string text)
        {
            text = text.Trim().ToLowerInvariant();

            return text switch
            {
                "normal" or "hitnormal" => HitSampleInfo.HIT_NORMAL,
                "whistle" or "hitwhistle" => HitSampleInfo.HIT_WHISTLE,
                "finish" or "hitfinish" => HitSampleInfo.HIT_FINISH,
                "clap" or "hitclap" => HitSampleInfo.HIT_CLAP,
                _ => string.Empty,
            };
        }

        private static IEnumerable<HitTarget> createTargets(EditorBeatmap editorBeatmap)
        {
            foreach (var hitObject in editorBeatmap.HitObjects)
            {
                yield return new HitTarget(hitObject, hitObject.StartTime, false);

                if (hitObject is not IHasRepeats repeats)
                    continue;

                populateNodeSamples(hitObject, repeats);
                yield return new HitTarget(hitObject, hitObject.StartTime + repeats.Duration, true, repeats.RepeatCount + 1);
            }
        }

        private static HitTarget? findNearestTarget(double time, IReadOnlyList<HitTarget> targets, double temporalLeniency)
        {
            int index = HitsoundTimeMatcher.FindNearest(targets, time, temporalLeniency, target => target.Time);
            return index < 0 ? null : targets[index];
        }

        private static void resetSamples(HitObject hitObject)
        {
            hitObject.Samples = new List<HitSampleInfo>();

            if (hitObject is not IHasRepeats repeats)
                return;

            populateNodeSamples(hitObject, repeats);

            for (int i = 0; i < repeats.NodeSamples.Count; i++)
                repeats.NodeSamples[i] = new List<HitSampleInfo>();
        }

        private static void populateNodeSamples(HitObject hitObject, IHasRepeats repeats)
        {
            while (repeats.NodeSamples.Count < repeats.RepeatCount + 2)
                repeats.NodeSamples.Add(hitObject.Samples.Select(sample => sample.With()).ToList());
        }

        private static void addOrReplaceSample(IList<HitSampleInfo> samples, HitSampleInfo sample)
        {
            // Legacy encoding expects at most one sample for each built-in hitsound name.
            // In particular, duplicate hitnormal samples cause getSampleBank() to throw
            // because it uses SingleOrDefault() to find the normal sample bank.
            for (int i = samples.Count - 1; i >= 0; i--)
            {
                if (samples[i].Name == sample.Name)
                    samples.RemoveAt(i);
            }

            samples.Add(sample);
        }

        private static void normaliseSamplesForStable(HitObject hitObject, string defaultNormalBank)
        {
            hitObject.Samples = normaliseSamplesForStable(hitObject.Samples, defaultNormalBank);

            if (hitObject is not IHasRepeats repeats)
                return;

            populateNodeSamples(hitObject, repeats);

            for (int i = 0; i < repeats.NodeSamples.Count; i++)
                repeats.NodeSamples[i] = normaliseSamplesForStable(repeats.NodeSamples[i], defaultNormalBank);
        }

        private static List<HitSampleInfo> normaliseSamplesForStable(IList<HitSampleInfo> samples, string defaultNormalBank)
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
                    customSamples.Add(sample);
                    continue;
                }

                builtInSamples[sample.Name] = sample;

                if (additionBank == null && isLegacyAdditionSample(sample.Name))
                    additionBank = normaliseBank(sample.Bank);
            }

            if (builtInSamples.Count == 0)
                return customSamples;

            string normalBank = defaultNormalBank;
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

        private static HitSampleInfo createStableSample(HitSampleInfo sample, string bank, int volume)
            => new HitSampleInfo(sample.Name, bank, sample.Suffix, volume, false);

        private static bool isLegacyBuiltInSample(string name)
            => name is HitSampleInfo.HIT_NORMAL or HitSampleInfo.HIT_WHISTLE or HitSampleInfo.HIT_FINISH or HitSampleInfo.HIT_CLAP;

        private static bool isLegacyAdditionSample(string name)
            => name is HitSampleInfo.HIT_WHISTLE or HitSampleInfo.HIT_FINISH or HitSampleInfo.HIT_CLAP;

        private readonly struct TimePosition
        {
            public readonly double Time;
            public readonly Vector2 Position;

            public TimePosition(double time, Vector2 position)
            {
                Time = time;
                Position = position;
            }
        }

        private readonly struct ManiaReferenceNote
        {
            public readonly double Time;
            public readonly int Column;

            public ManiaReferenceNote(double time, int column)
            {
                Time = time;
                Column = column;
            }
        }

        private sealed class HitTarget
        {
            public readonly HitObject HitObject;
            public readonly double Time;
            private readonly bool sliderEnd;
            private readonly int nodeIndex;

            public HitTarget(HitObject hitObject, double time, bool sliderEnd, int nodeIndex = 0)
            {
                HitObject = hitObject;
                Time = time;
                this.sliderEnd = sliderEnd;
                this.nodeIndex = nodeIndex;
            }

            public void AddSample(HitSampleInfo sample) => addOrReplaceSample(getMutableSamples(), sample);

            private IList<HitSampleInfo> getMutableSamples()
            {
                if (sliderEnd && HitObject is IHasRepeats repeats)
                {
                    var samples = repeats.NodeSamples[nodeIndex];

                    if (samples.IsReadOnly)
                        repeats.NodeSamples[nodeIndex] = samples = samples.ToList();

                    return samples;
                }

                if (HitObject.Samples.IsReadOnly)
                    HitObject.Samples = HitObject.Samples.ToList();

                return HitObject.Samples;
            }
        }

        private sealed class ReferenceHitObject : HitObject, IHasPosition
        {
            public float X
            {
                get => Position.X;
                set => Position = new Vector2(value, Position.Y);
            }

            public float Y
            {
                get => Position.Y;
                set => Position = new Vector2(Position.X, value);
            }

            public Vector2 Position { get; set; }

            public override Judgement CreateJudgement() => new IgnoreJudgement();

            protected override HitWindows CreateHitWindows() => HitWindows.Empty;
        }
    }
}
