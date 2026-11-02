// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace osu.Game.Screens.Edit.Components
{
    /// <summary>Side-by-side preview and controls on wide screens; stacked at readable widths.</summary>
    public partial class EditorToolSplitPane : Container
    {
        private readonly Drawable first;
        private readonly Drawable second;
        private readonly float paneHeight;
        private readonly float firstFraction;

        public EditorToolSplitPane(float height, float firstFraction, Drawable first, Drawable second)
        {
            this.first = first;
            this.second = second;
            paneHeight = height;
            this.firstFraction = firstFraction;
            RelativeSizeAxes = Axes.X;
            Height = height;
            first.RelativeSizeAxes = second.RelativeSizeAxes = Axes.None;
            first.Anchor = first.Origin = second.Anchor = second.Origin = Anchor.TopLeft;
            Children = new[] { first, second };
        }

        protected override void Update()
        {
            base.Update();
            bool stacked = DrawWidth < 640;
            Height = stacked ? paneHeight * 2 + 8 : paneHeight;
            first.Position = Vector2.Zero;
            first.Size = new Vector2(stacked ? DrawWidth : (DrawWidth - 8) * firstFraction, paneHeight);
            second.Position = stacked ? new Vector2(0, paneHeight + 8) : new Vector2(first.Width + 8, 0);
            second.Size = new Vector2(stacked ? DrawWidth : DrawWidth - first.Width - 8, paneHeight);
        }
    }
}
