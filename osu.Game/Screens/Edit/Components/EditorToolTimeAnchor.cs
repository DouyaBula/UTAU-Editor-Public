// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Rulesets.Edit;

namespace osu.Game.Screens.Edit.Components
{
    /// <summary>Explicit insertion time; seeking the editor never silently moves an active preview.</summary>
    public partial class EditorToolTimeAnchor : EditorToolControlRow
    {
        public readonly BindableDouble Current = new BindableDouble { MinValue = 0, MaxValue = 86400000 };

        [Resolved] private EditorClock clock { get; set; } = null!;
        [Resolved] private IBeatSnapProvider snap { get; set; } = null!;

        public EditorToolTimeAnchor()
        {
            Children = new Drawable[]
            {
                new FormSliderBar<double>
                {
                    Caption = "Insert at (ms)",
                    HintText = "Locked while this panel is open. Seek, then use the button to move the preview explicitly.",
                    Current = Current,
                    KeyboardStep = 1,
                    LabelFormat = value => $"{value:0.##} ms",
                },
                new RoundedButton { Text = "Use timeline cursor", Height = 40, Action = capture },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            capture();
        }

        private void capture() => Current.Value = snap.SnapTime(clock.CurrentTime);
    }
}
