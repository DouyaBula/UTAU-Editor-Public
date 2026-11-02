// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Screens.Edit.Components;

namespace osu.Game.Screens.Edit.Timing
{
    public partial class ControlPointSettings : EditorRoundedScreenSettings
    {
        protected override IReadOnlyList<Drawable> CreateSections() => new Drawable[]
        {
            new EditorToolButton("Global offset...",
                () => new SpriteIcon { Icon = FontAwesome.Solid.ArrowsAltH },
                () => new GlobalOffsetPopover()),
            new GroupSection(),
            new TimingSection(),
            new EffectSection(),
        };
    }
}
