// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Edit
{
    internal class EditorToggleMenuItem : ToggleMenuItem
    {
        [UsedImplicitly]
        private readonly Bindable<bool> current;

        public EditorToggleMenuItem(LocalisableString text, Bindable<bool> current)
            : base(text)
        {
            State.BindTo(this.current = current);
        }
    }
}
