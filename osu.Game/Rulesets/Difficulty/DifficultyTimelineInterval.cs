// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Difficulty
{
    /// <summary>
    /// A time interval carrying the local difficulty value of one skill.
    /// </summary>
    public readonly struct DifficultyTimelineInterval
    {
        public int SkillId { get; }
        public string SkillName { get; }
        public double StartTime { get; }
        public double EndTime { get; }
        public double Value { get; }
        public double SkillWeight { get; }

        public DifficultyTimelineInterval(int skillId, string skillName, double startTime, double endTime, double value, double skillWeight)
        {
            SkillId = skillId;
            SkillName = skillName;
            StartTime = startTime;
            EndTime = endTime;
            Value = value;
            SkillWeight = skillWeight;
        }
    }
}
