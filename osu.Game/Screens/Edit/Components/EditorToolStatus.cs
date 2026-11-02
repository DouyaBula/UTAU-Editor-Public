// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;

namespace osu.Game.Screens.Edit.Components
{
    /// <summary>Wrapping tool feedback, including validation errors, at the panel's actual width.</summary>
    public partial class EditorToolStatus : OsuTextFlowContainer
    {
        public EditorToolStatus()
            : base(text => text.Font = OsuFont.Default.With(size: 12))
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
        }
    }
}
