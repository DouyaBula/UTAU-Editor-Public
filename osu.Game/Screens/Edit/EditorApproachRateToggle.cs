// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Screens.Edit
{
    public static class EditorApproachRateToggle
    {
        public static bool Toggle(EditorBeatmap editorBeatmap, float valueA, float valueB)
        {
            valueA = Math.Clamp(valueA, 0f, 10f);
            valueB = Math.Clamp(valueB, 0f, 10f);

            float current = editorBeatmap.Difficulty.ApproachRate;
            float next = Math.Abs(current - valueA) < 0.001f ? valueB : valueA;

            if (Math.Abs(current - next) < 0.001f)
                return true;

            editorBeatmap.Difficulty.ApproachRate = next;
            editorBeatmap.UpdateAllHitObjects();
            editorBeatmap.SaveState();

            return true;
        }
    }
}
