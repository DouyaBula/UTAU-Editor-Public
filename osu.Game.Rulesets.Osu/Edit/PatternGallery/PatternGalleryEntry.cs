// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Osu.Edit.PatternGallery
{
    /// <summary>
    /// A reusable pattern and the small amount of metadata needed to present it in the gallery.
    /// The object payload is the editor's normal typed clipboard representation, so the pattern
    /// remains made up of ordinary osu! hit objects when it is pasted back into a beatmap.
    /// </summary>
    public sealed class PatternGalleryEntry
    {
        public int FormatVersion { get; set; } = 2;
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Tags { get; set; } = string.Empty;
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        public DateTime LastUsedUtc { get; set; } = DateTime.UtcNow;
        public int UseCount { get; set; }
        public int ObjectCount { get; set; }
        public double Duration { get; set; }
        public string SerializedObjects { get; set; } = string.Empty;

        /// <summary>
        /// Full beat length at the first selected object in the source map. The values below are
        /// expressed in beats rather than milliseconds so patterns can be inserted into maps with
        /// a different BPM without losing their snap rhythm.
        /// </summary>
        public double SourceBeatLength { get; set; }

        public List<double> RelativeStartBeats { get; set; } = new List<double>();
        public List<double> DurationBeats { get; set; } = new List<double>();

        public bool HasTimingMetadata
            => SourceBeatLength > 0
               && RelativeStartBeats?.Count == ObjectCount
               && DurationBeats?.Count == ObjectCount;
    }
}
