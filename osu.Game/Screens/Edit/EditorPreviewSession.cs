// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Screens.Edit
{
    /// <summary>
    /// A one-shot preview transaction. Closing it invalidates queued preview work before rolling
    /// back, and balances Begin/End exactly once, including re-entrant close notifications.
    /// </summary>
    public sealed class EditorPreviewSession
    {
        private IEditorChangeHandler? handler;
        private bool started;

        public bool IsClosed { get; private set; }

        public bool TryBegin(IEditorChangeHandler? changeHandler)
        {
            if (IsClosed)
                return false;

            if (!started)
            {
                changeHandler?.BeginChange();
                handler = changeHandler;
                started = true;
            }

            return true;
        }

        public void Commit() => close(null);

        public void Cancel(Action restore) => close(restore);

        private void close(Action? restore)
        {
            if (IsClosed)
                return;

            IsClosed = true;
            if (!started)
                return;

            try
            {
                restore?.Invoke();
            }
            finally
            {
                handler?.EndChange();
            }
        }
    }
}
