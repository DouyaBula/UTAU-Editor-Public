// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace osu.Game.Screens.Edit.Components
{
    /// <summary>Equal-width controls that wrap before their labels and numeric inputs become cramped.</summary>
    public partial class EditorToolControlRow : FillFlowContainer
    {
        private readonly float minimumControlWidth;

        public EditorToolControlRow(float minimumControlWidth = 150)
        {
            this.minimumControlWidth = minimumControlWidth;
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Direction = FillDirection.Full;
            Spacing = new Vector2(6);
        }

        protected override void Update()
        {
            base.Update();
            if (DrawWidth <= 0 || Children.Count == 0)
                return;

            int columns = Math.Clamp((int)((DrawWidth + Spacing.X) / (minimumControlWidth + Spacing.X)), 1, Children.Count);
            float fraction = Math.Max(0.01f, (DrawWidth - Spacing.X * (columns - 1) - 1) / columns / DrawWidth);
            foreach (var child in Children)
            {
                child.RelativeSizeAxes |= Axes.X;
                child.Width = fraction;
            }
        }
    }
}
