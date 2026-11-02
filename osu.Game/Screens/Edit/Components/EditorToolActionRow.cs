// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterfaceV2;

namespace osu.Game.Screens.Edit.Components
{
    /// <summary>Persistent actions, lifted out of the scrolling body by EditorPopover.</summary>
    public partial class EditorToolActionRow : GridContainer
    {
        public EditorToolActionRow(RoundedButton apply, Action cancel)
        {
            RelativeSizeAxes = Axes.X;
            Height = 42;
            Padding = new MarginPadding { Top = 6 };
            apply.RelativeSizeAxes = Axes.Both;
            apply.Size = new osuTK.Vector2(1);
            apply.Text = "Apply";
            ColumnDimensions = new[] { new Dimension(), new Dimension(GridSizeMode.Absolute, 8), new Dimension() };
            Content = new[]
            {
                new Drawable[]
                {
                    apply,
                    new Container(),
                    new RoundedButton { RelativeSizeAxes = Axes.Both, Size = new osuTK.Vector2(1), Text = "Cancel", Action = cancel },
                },
            };
        }
    }
}
