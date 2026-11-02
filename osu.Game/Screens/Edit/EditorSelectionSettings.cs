// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Extensions.Color4Extensions;
using osuTK.Graphics;

namespace osu.Game.Screens.Edit
{
    public static class EditorSelectionSettings
    {
        public static Color4 ParseColour(string? hex, Color4 fallback)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(hex))
                    return fallback;

                return Color4Extensions.FromHex(NormaliseHex(hex));
            }
            catch
            {
                return fallback;
            }
        }

        public static string NormaliseHex(string hex)
        {
            hex = hex.Trim();

            if (hex.StartsWith('#'))
                hex = hex[1..];

            return hex.ToLowerInvariant();
        }

        public static Color4 ForegroundFor(Color4 colour)
        {
            float luminance = 0.299f * colour.R + 0.587f * colour.G + 0.114f * colour.B;
            return luminance > 0.55f ? Color4.Black : Color4.White;
        }
    }
}
