// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterfaceV2;
using osuTK;

namespace osu.Game.Screens.Edit.Components
{
    /// <summary>Task-based pages that retain form values when switching workflows.</summary>
    public partial class EditorToolPages : FillFlowContainer
    {
        public readonly Bindable<string> Current = new Bindable<string>();

        public EditorToolPages(params (string Title, Drawable[] Controls)[] pages)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Direction = FillDirection.Vertical;
            Spacing = new Vector2(6);

            Current.Value = pages[0].Title;
            Add(new FormDropdown<string>
            {
                Caption = "Task",
                Items = pages.Select(p => p.Title),
                Current = Current,
            });

            foreach (var page in pages)
            {
                var body = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(6),
                    Children = page.Controls,
                };
                Add(body);
                Current.BindValueChanged(value => body.Alpha = value.NewValue == page.Title ? 1 : 0, true);
            }
        }
    }
}
