// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterfaceV2;
using osuTK;

namespace osu.Game.Screens.Edit.Components
{
    /// <summary>
    /// An editor tool panel bounded by its actual popover host, with scrolling and an independent
    /// host for child popovers so opening a file selector does not replace the tool itself.
    /// </summary>
    public partial class EditorPopover : OsuPopover
    {
        private PopoverContainer viewport = null!;
        private Drawable panel = null!;
        private float preferredWidth;
        private float preferredHeight = 560;
        private float footerHeight;

        public EditorPopover(bool withPadding = true)
            : base(withPadding)
        {
        }

        protected override void LoadComplete()
        {
            // All tool panels have a single root; normalise the old fixed-height scroll wrappers
            // here so they don't become nested scroll areas after adding the bounded viewport.
            panel = Children.Single();
            Remove(panel, false);
            preferredWidth = panel.Width;

            if (panel is OsuScrollContainer existingScroll)
            {
                preferredHeight = Math.Min(preferredHeight, existingScroll.Height);
                panel = existingScroll.Child;
                existingScroll.Remove(panel, false);
                existingScroll.Dispose();
            }

            panel.RelativeSizeAxes = Axes.X;
            panel.Width = 1;

            var footer = (panel as Container<Drawable>)?.Children.OfType<EditorToolActionRow>().SingleOrDefault();
            if (footer?.Parent is Container<Drawable> footerParent)
            {
                footerParent.Remove(footer, false);
                footerHeight = footer.Height;
            }

            var scroll = new PanelScrollContainer
            {
                RelativeSizeAxes = Axes.Both,
                ScrollbarOverlapsContent = false,
                Child = panel,
            };

            Child = viewport = new PopoverContainer
            {
                Size = new Vector2(preferredWidth, 1),
                Child = new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    RowDimensions = new[] { new Dimension(), new Dimension(GridSizeMode.Absolute, footerHeight) },
                    Content = new[] { new Drawable[] { scroll }, new Drawable[] { footer ?? (Drawable)new Container() } },
                },
            };

            base.LoadComplete();
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            var host = this.FindClosestParent<PopoverContainer>();
            if (host == null || Parent == null)
                return;

            // Use the parent's coordinate system: the pop-in scale animation must not feed back
            // into layout and repeatedly shrink/grow the scroll viewport.
            Vector2 available = Parent.ToLocalSpace(host.ScreenSpaceDrawQuad).AABBFloat.Size;
            available -= new Vector2(Content.Padding.TotalHorizontal + Body.Margin.TotalHorizontal + 16,
                Content.Padding.TotalVertical + Body.Margin.TotalVertical + 16);

            viewport.Size = new Vector2(
                Math.Max(1, Math.Min(preferredWidth, available.X)),
                Math.Max(1, Math.Min(panel.DrawHeight + footerHeight, Math.Min(preferredHeight, available.Y))));
        }

        private partial class PanelScrollContainer : OsuScrollContainer
        {
            private double previousExtent;

            protected override void UpdateAfterChildren()
            {
                // Keep the last row reachable while auto-sized groups finish expanding, or the
                // editor is resized. Do not pull users to the end when they are scrolling elsewhere.
                bool wasAtEnd = previousExtent > 0 && Math.Abs(Target - previousExtent) < 1;
                base.UpdateAfterChildren();
                if (wasAtEnd && previousExtent != ScrollableExtent)
                    ScrollToEnd(false);
                previousExtent = ScrollableExtent;
            }
        }
    }
}
