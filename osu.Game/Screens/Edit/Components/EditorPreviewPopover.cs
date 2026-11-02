// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Screens.Edit.Components
{
    /// <summary>
    /// Common lifecycle for live, cancellable editor tools. Preview writers must call
    /// <see cref="BeginPreview"/> before touching the beatmap, including scheduled callbacks.
    /// </summary>
    public abstract partial class EditorPreviewPopover : EditorPopover
    {
        [Resolved(canBeNull: true)]
        private IEditorChangeHandler? changeHandler { get; set; }

        [Resolved]
        private EditorBeatmap beatmap { get; set; } = null!;

        private readonly EditorPreviewSession session = new EditorPreviewSession();
        private bool opened;
        private BreakPeriod[]? originalBreaks;
        private int originalBeatmapVersion;
        private (IHasComboInformation Object, bool NewCombo, int Offset)[] originalCombos = [];

        protected bool PreviewClosed => session.IsClosed;

        protected EditorPreviewPopover(bool withPadding = true)
            : base(withPadding)
        {
        }

        protected bool BeginPreview()
        {
            if (!session.TryBegin(changeHandler))
                return false;

            if (originalBreaks == null)
            {
                originalBreaks = beatmap.Breaks.ToArray();
                originalBeatmapVersion = beatmap.BeatmapVersion;
                originalCombos = beatmap.HitObjects.OfType<IHasComboInformation>().Select(h => (h, h.NewCombo, h.ComboOffset)).ToArray();
            }

            return true;
        }

        protected void ReplacePreviewObjects<T>(List<T> current, IEnumerable<T> next) where T : HitObject
        {
            var replacement = next.ToArray();
            beatmap.BeginChange();
            try
            {
                beatmap.RemoveRange(current);
                current.Clear();
                current.AddRange(replacement);
                beatmap.AddRange(current);
            }
            finally
            {
                beatmap.EndChange();
            }
        }

        protected void CommitPreview()
        {
            session.Commit();
            Hide();
        }

        protected abstract void RestorePreview();

        protected override void PopIn()
        {
            opened = true;
            base.PopIn();
        }

        protected override void PopOut()
        {
            // VisibilityContainer can emit an initial PopOut while loading a hidden panel.
            if (opened)
                session.Cancel(() =>
                {
                    RestorePreview();
                    foreach (var original in originalCombos)
                    {
                        if (original.Object.NewCombo == original.NewCombo && original.Object.ComboOffset == original.Offset)
                            continue;
                        original.Object.NewCombo = original.NewCombo;
                        original.Object.ComboOffset = original.Offset;
                        beatmap.Update((HitObject)original.Object);
                    }
                    // Processing a transient preview can regenerate breaks and upgrade the file
                    // version. Cancellation must restore these indirect edits as well.
                    beatmap.FlushPendingUpdates();
                    if (originalBreaks != null)
                    {
                        beatmap.Breaks.Clear();
                        beatmap.Breaks.AddRange(originalBreaks);
                        beatmap.BeatmapVersion = originalBeatmapVersion;
                    }
                });

            base.PopOut();
        }
    }
}
