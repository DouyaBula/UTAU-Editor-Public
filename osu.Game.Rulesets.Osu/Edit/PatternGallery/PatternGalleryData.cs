// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;

namespace osu.Game.Rulesets.Osu.Edit.PatternGallery
{
    /// <summary>On-disk envelope for the editor-wide pattern gallery.</summary>
    public sealed class PatternGalleryData
    {
        public int FormatVersion { get; set; } = 2;
        public List<PatternGalleryEntry> Patterns { get; set; } = new List<PatternGalleryEntry>();
    }
}
