// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit;

namespace osu.Game.Rulesets.Osu.Edit.Curves
{
    /// <summary>
    /// Applies the common timing policy for geometry edits which preserve a slider's duration.
    /// The expected distance is set to the native path length so an earlier beat snap cannot
    /// quantise or truncate the edited geometry.
    /// </summary>
    internal static class SliderDurationPreserver
    {
        public static bool TryApply(Slider slider, double originalDistance, double originalVelocity, EditorBeatmap? beatmap, out double newDistance)
        {
            newDistance = 0;

            if (originalDistance <= 0 || !double.IsFinite(originalDistance) || !double.IsFinite(originalVelocity))
                return false;

            double? previousExpectedDistance = slider.Path.ExpectedDistance.Value;
            slider.Path.ExpectedDistance.Value = null;

            newDistance = slider.Path.CalculatedDistance;
            double newVelocity = originalVelocity * newDistance / originalDistance;

            if (!double.IsFinite(newDistance) || newDistance < 0.01
                || !double.IsFinite(newVelocity)
                || newVelocity < slider.SliderVelocityMultiplierBindable.MinValue
                || newVelocity > slider.SliderVelocityMultiplierBindable.MaxValue)
            {
                slider.Path.ExpectedDistance.Value = previousExpectedDistance;
                return false;
            }

            slider.Path.ExpectedDistance.Value = newDistance;
            slider.SliderVelocityMultiplier = newVelocity;
            if (beatmap != null)
                slider.UpdateVelocity(beatmap.ControlPointInfo, beatmap.Difficulty);
            return true;
        }
    }
}
