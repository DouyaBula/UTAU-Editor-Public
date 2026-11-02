// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    public readonly struct DifficultyRatingSample
    {
        public readonly double Time;
        public readonly double RelativeDifficulty;
        public readonly string SkillName;

        public DifficultyRatingSample(double time, double relativeDifficulty, string skillName)
        {
            Time = time;
            RelativeDifficulty = relativeDifficulty;
            SkillName = skillName;
        }
    }
}
