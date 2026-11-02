// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Audio;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Screens.Edit
{
    public static class EditorHitsoundTools
    {
        public const int MUTED_VOLUME = 5;

        public static int MuteAllSliderEnds(EditorBeatmap editorBeatmap) =>
            muteSliderEnds(editorBeatmap, editorBeatmap.HitObjects);

        public static int MuteSelectedSliderEnds(EditorBeatmap editorBeatmap) =>
            muteSliderEnds(editorBeatmap, editorBeatmap.SelectedHitObjects);

        public static int SetSelectedObjectVolume(EditorBeatmap editorBeatmap, int volume, bool preserveMuted)
        {
            int changed = 0;

            foreach (var hitObject in editorBeatmap.SelectedHitObjects)
            {
                bool objectChanged = setVolume(hitObject.Samples, volume, preserveMuted);

                if (hitObject is IHasRepeats repeats)
                {
                    populateNodeSamples(hitObject, repeats);

                    foreach (var nodeSamples in repeats.NodeSamples)
                        objectChanged |= setVolume(nodeSamples, volume, preserveMuted);
                }

                if (!objectChanged)
                    continue;

                editorBeatmap.Update(hitObject);
                changed++;
            }

            return changed;
        }

        private static int muteSliderEnds(EditorBeatmap editorBeatmap, IEnumerable<HitObject> hitObjects)
        {
            int changed = 0;

            foreach (var hitObject in hitObjects)
            {
                if (hitObject is not IHasRepeats repeats)
                    continue;

                populateNodeSamples(hitObject, repeats);

                if (!setVolume(repeats.NodeSamples[repeats.RepeatCount + 1], MUTED_VOLUME, false))
                    continue;

                editorBeatmap.Update(hitObject);
                changed++;
            }

            return changed;
        }

        private static bool setVolume(IList<HitSampleInfo> samples, int volume, bool preserveMuted)
        {
            if (samples.Count == 0)
            {
                samples.Add(new HitSampleInfo(HitSampleInfo.HIT_NORMAL, volume: volume));
                return true;
            }

            bool changed = false;

            for (int i = 0; i < samples.Count; i++)
            {
                if (preserveMuted && samples[i].Volume == MUTED_VOLUME)
                    continue;

                if (samples[i].Volume == volume)
                    continue;

                samples[i] = samples[i].With(newVolume: volume);
                changed = true;
            }

            return changed;
        }

        private static void populateNodeSamples(HitObject hitObject, IHasRepeats repeats)
        {
            while (repeats.NodeSamples.Count < repeats.RepeatCount + 2)
                repeats.NodeSamples.Add(hitObject.Samples.Select(sample => sample.With()).ToList());
        }
    }
}
