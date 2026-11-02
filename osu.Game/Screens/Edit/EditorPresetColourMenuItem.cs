// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;
using osu.Framework.Localisation;

namespace osu.Game.Screens.Edit
{
    internal class EditorPresetColourMenuItem : MenuItem
    {
        private readonly Bindable<string> current;
        private readonly Dictionary<string, TernaryStateRadioMenuItem> menuItemLookup = new Dictionary<string, TernaryStateRadioMenuItem>();

        public EditorPresetColourMenuItem(LocalisableString text, Bindable<string> current)
            : base(text)
        {
            Items = new[]
            {
                createMenuItem("Yellow", "ffcc22"),
                createMenuItem("Orange", "eeaa00"),
                createMenuItem("Red", "ff5a5a"),
                createMenuItem("Stable red", "ed1121"),
                createMenuItem("Pink", "ff66aa"),
                createMenuItem("Light pink", "ff99cc"),
                createMenuItem("Pale pink", "ffddee"),
                createMenuItem("Purple", "8866ee"),
                createMenuItem("Perfect purple", "6644cc"),
                createMenuItem("Blue", "66ccff"),
                createMenuItem("Cyan", "05f4fd"),
                createMenuItem("Seafoam", "05ffa2"),
                createMenuItem("Green", "88b300"),
                createMenuItem("Light grey", "c8c8c8"),
                createMenuItem("White", "ffffff"),
            };

            this.current = current;
            current.BindValueChanged(colour =>
            {
                string normalised = EditorSelectionSettings.NormaliseHex(colour.NewValue);

                foreach (var kvp in menuItemLookup)
                    kvp.Value.State.Value = kvp.Key == normalised ? TernaryState.True : TernaryState.False;
            }, true);
        }

        private TernaryStateRadioMenuItem createMenuItem(string name, string hex)
        {
            string normalised = EditorSelectionSettings.NormaliseHex(hex);
            var item = new TernaryStateRadioMenuItem($"{name} (#{normalised})", MenuItemType.Standard, _ => current.Value = normalised);
            menuItemLookup[normalised] = item;
            return item;
        }
    }
}
