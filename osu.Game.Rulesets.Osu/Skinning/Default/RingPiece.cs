// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Osu.Objects;
using osuTK.Graphics;
using osuTK;

namespace osu.Game.Rulesets.Osu.Skinning.Default
{
    public partial class RingPiece : CircularContainer
    {
        private readonly bool innerOnly;

        public float Thickness
        {
            get => BorderThickness;
            set
            {
                BorderThickness = value;

                // Original outer/normal sizing:
                // Size = OsuHitObject.OBJECT_DIMENSIONS;

                Size = innerOnly
                    ? OsuHitObject.OBJECT_DIMENSIONS - new Vector2(value)
                    : OsuHitObject.OBJECT_DIMENSIONS;
            }
        }

        public RingPiece(float thickness = 9, bool innerOnly = false)
        {
            this.innerOnly = innerOnly;

            // Original:
            // Size = OsuHitObject.OBJECT_DIMENSIONS;

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Masking = true;
            BorderColour = Color4.White;

            Thickness = thickness;

            Child = new Box
            {
                AlwaysPresent = true,
                Alpha = 0,
                RelativeSizeAxes = Axes.Both
            };
        }
    }
}
