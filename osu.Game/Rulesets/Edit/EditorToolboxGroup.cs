// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Overlays;

namespace osu.Game.Rulesets.Edit
{
    public partial class EditorToolboxGroup : SettingsToolboxGroup
    {
        /// <summary>
        /// A stable identifier used when persisting the group order.
        /// </summary>
        public string OrderKey { get; }

        public EditorToolboxGroup(string title, string? orderKey = null)
            : base(title)
        {
            RelativeSizeAxes = Axes.X;
            Width = 1;
            OrderKey = orderKey ?? $"{GetType().FullName ?? nameof(EditorToolboxGroup)}:{title}";
        }

    }
}
