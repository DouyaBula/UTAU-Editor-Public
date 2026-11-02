// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays.Dialog;
using osu.Game.Rulesets.Edit;
using osuTK;

namespace osu.Game.Screens.Edit
{
    /// <summary>
    /// A deterministic alternative to header dragging for arranging editor toolbox groups.
    /// The explicit move buttons are intentionally usable even when a scroll container or a
    /// group control captures the mouse gesture.
    /// </summary>
    public partial class EditorToolboxOrderDialog : PopupDialog
    {
        private readonly HitObjectComposer composer;
        private readonly FillFlowContainer columns;

        public EditorToolboxOrderDialog(HitObjectComposer composer)
        {
            this.composer = composer;

            HeaderText = "Toolbox order";
            Icon = FontAwesome.Solid.Bars;

            MainContent.Add(columns = new FillFlowContainer
            {
                // DialogOverlay is 500px wide. Keep both columns inside that hit-test region;
                // otherwise the right column is interpreted as an outside click and dismisses
                // the dialog before its buttons can receive input.
                Width = 480,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(10),
            });

            Buttons = new PopupDialogButton[]
            {
                new PopupDialogCancelButton
                {
                    Text = "Close",
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // The compose screen and its toolbox children are loaded asynchronously. Building
            // the list in the constructor can therefore observe half-created containers when the
            // menu is clicked immediately after entering the editor.
            rebuild();
        }

        private void rebuild()
        {
            columns.Clear();

            var toolboxes = composer.ToolboxContainers;

            if (toolboxes == null)
                return;

            bool waitingForToolbox = false;

            for (int side = 0; side < toolboxes.Count; side++)
            {
                var toolbox = toolboxes[side];

                if (toolbox == null)
                    continue;

                if (!toolbox.IsLoaded)
                {
                    waitingForToolbox = true;
                    continue;
                }

                columns.Add(createColumn(toolbox, side == 0 ? "Left sidebar" : "Right sidebar"));
            }

            if (waitingForToolbox)
                this.Delay(100).Schedule(rebuild);
        }

        private Drawable createColumn(ExpandingToolboxContainer toolbox, string title)
        {
            var column = new FillFlowContainer
            {
                Width = 235,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 6),
            };

            column.Add(new OsuSpriteText
            {
                Text = title,
                Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 18),
                Margin = new MarginPadding { Bottom = 4 },
            });

            var groups = toolbox.ReorderableGroups;

            for (int groupIndex = 0; groupIndex < groups.Count; groupIndex++)
            {
                var group = groups[groupIndex];
                string key = group.OrderKey;
                column.Add(new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 34,
                    Children = new Drawable[]
                    {
                        new TruncatingSpriteText
                        {
                            Text = group.Title,
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            X = 8,
                            MaxWidth = 145,
                        },
                        new RoundedButton
                        {
                            Text = "↑",
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            X = -62,
                            Width = 28,
                            Height = 28,
                            Enabled = { Value = groupIndex > 0 },
                            Action = () =>
                            {
                                toolbox.MoveGroup(key, -1);
                                // Do not rebuild the popup hierarchy while the button is still
                                // dispatching its click event. Queueing the refresh avoids
                                // invalidating the active drawable tree under the input manager.
                                Schedule(rebuild);
                            },
                        },
                        new RoundedButton
                        {
                            Text = "↓",
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            X = -3,
                            Width = 28,
                            Height = 28,
                            Enabled = { Value = groupIndex < groups.Count - 1 },
                            Action = () =>
                            {
                                toolbox.MoveGroup(key, 1);
                                Schedule(rebuild);
                            },
                        },
                    },
                });
            }

            return column;
        }
    }
}
