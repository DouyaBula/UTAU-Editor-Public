// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Edit
{
    internal class EditorFloatMenuItem : MenuItem
    {
        private readonly Bindable<float> current;
        private readonly string suffix;
        private readonly Dictionary<float, TernaryStateRadioMenuItem> menuItemLookup = new Dictionary<float, TernaryStateRadioMenuItem>();

        public EditorFloatMenuItem(LocalisableString text, Bindable<float> current, params float[] values)
            : this(text, current, "px", values)
        {
        }

        public EditorFloatMenuItem(LocalisableString text, Bindable<float> current, string suffix, params float[] values)
            : base(text)
        {
            this.suffix = suffix;

            var items = new List<MenuItem>();

            foreach (float value in values)
                items.Add(createMenuItem(value));

            Items = items;

            this.current = current;
            current.BindValueChanged(size =>
            {
                foreach (var kvp in menuItemLookup)
                    kvp.Value.State.Value = kvp.Key == size.NewValue ? TernaryState.True : TernaryState.False;
            }, true);
        }

        private TernaryStateRadioMenuItem createMenuItem(float value)
        {
            var item = new TernaryStateRadioMenuItem($"{value:g}{suffix}", MenuItemType.Standard, _ => current.Value = value);
            menuItemLookup[value] = item;
            return item;
        }
    }
}
