// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Extensions.Color4Extensions;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Skinning.Default;
using osu.Game.Skinning;
using osu.Game.Utils;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning.Legacy
{
    public partial class LegacySliderBody : PlaySliderBody
    {
        protected override DrawableSliderPath CreateSliderPath() => new LegacyDrawableSliderPath();

        protected override Color4 GetBorderColour(ISkinSource skin)
            => skin.GetConfig<OsuSkinColour, Color4>(OsuSkinColour.SliderBorder)?.Value ?? Color4.White;

        protected override Color4 GetBodyAccentColour(ISkinSource skin, Color4 hitObjectAccentColour)
            // legacy skins use a constant value for slider track alpha, regardless of the source colour.
            => (skin.GetConfig<OsuSkinColour, Color4>(OsuSkinColour.SliderTrackOverride)?.Value ?? hitObjectAccentColour).Opacity(0.7f);

        private partial class LegacyDrawableSliderPath : DrawableSliderPath
        {
            protected override Color4 ColourAt(float position)
            {
                // Applying stable's anti-aliasing width makes sliders blurry, especially on CS > 5.
                const float aa_width = 0f;

                Color4 outerColour = AccentColour.Darken(0.1f);
                Color4 innerColour = lighten(AccentColour, 0.5f);

                const float shadow_portion = 1 - (OsuLegacySkinTransformer.LEGACY_CIRCLE_RADIUS / OsuHitObject.OBJECT_RADIUS);
                const float border_portion = 0.1875f;

                if (position <= shadow_portion - aa_width)
                    return new Color4(0, 0, 0, 0f);

                if (position <= shadow_portion + aa_width)
                    return new Color4(0, 0, 0, 0f);

                if (position <= border_portion - aa_width)
                    return BorderColour;

                if (position <= border_portion + aa_width)
                    return new Color4(0, 0, 0, 0f);

                if (!RenderBody.Value)
                    return new Color4(0, 0, 0, 0f);

                float progress = (position - border_portion) / (1 - border_portion);
                return new Color4(
                    outerColour.R + (innerColour.R - outerColour.R) * progress,
                    outerColour.G + (innerColour.G - outerColour.G) * progress,
                    outerColour.B + (innerColour.B - outerColour.B) * progress,
                    AccentColour.A);
            }

            /// <summary>
            /// Lightens a colour in a way more friendly to dark or strong colours.
            /// </summary>
            private static Color4 lighten(Color4 color, float amount)
            {
                amount *= 0.5f;
                return new Color4(
                    Math.Min(1, color.R * (1 + 0.5f * amount) + amount),
                    Math.Min(1, color.G * (1 + 0.5f * amount) + amount),
                    Math.Min(1, color.B * (1 + 0.5f * amount) + amount),
                    color.A);
            }
        }
    }
}
