// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;

namespace osu.Game.Screens.Edit
{
    internal class SelectionOutlineThicknessMenuItem : MenuItem
    {
        private readonly Bindable<float> outlineThickness;

        private readonly Dictionary<float, TernaryStateRadioMenuItem> menuItemLookup = new Dictionary<float, TernaryStateRadioMenuItem>();

        public SelectionOutlineThicknessMenuItem(Bindable<float> outlineThickness)
            : base(EditorStrings.SelectionOutlineThickness)
        {
            Items = new[]
            {
                createMenuItem(0f),
                createMenuItem(2f),
                createMenuItem(4f),
                createMenuItem(6f),
                createMenuItem(9f),
                createMenuItem(12f),
                createMenuItem(16f),
                createMenuItem(24f),
            };

            this.outlineThickness = outlineThickness;
            outlineThickness.BindValueChanged(thickness =>
            {
                foreach (var kvp in menuItemLookup)
                    kvp.Value.State.Value = kvp.Key == thickness.NewValue ? TernaryState.True : TernaryState.False;
            }, true);
        }

        private TernaryStateRadioMenuItem createMenuItem(float thickness)
        {
            var item = new TernaryStateRadioMenuItem($"{thickness}px", MenuItemType.Standard, _ => updateThickness(thickness));
            menuItemLookup[thickness] = item;
            return item;
        }

        private void updateThickness(float thickness) => outlineThickness.Value = thickness;
    }
}
