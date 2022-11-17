// Stats.Values contains a library of value types that store constant data and methods for processing many stat values.
// This avoids encapsulating lengthy code within the stat classes themselves and encapsulates any hard values
// in one place (e.g. Global.Attribute.AVERAGE).
using System;
using System.Collections.Generic;

namespace Stats.Values
{
    public struct Global
    {
        public struct Attribute { public const int AVERAGE = 50; }
        public struct Organ { public const float BLEED_THR_MULTIPLIER = 0.6f; }
    }

    /// <summary>
    /// A collection of constant values for calculating recovery rates for Vitals
    /// </summary>
    public struct Recovery // fixme: change recovery rates to a % of max (time constant = how long it takes to reach 100% from 0)
    {
        /// <summary>
        /// The time increment at which recovery coroutines iterate. (30 pulses/sec)
        /// </summary>
        public const float PULSE_TIME = 0.033f;

        /*VITALS*/
        const float VITAL_PTS_RECOVERED_PER_SEC = 2f;
        /// <summary>
        /// 
        /// </summary>
        public static float AvgVitalPtsPerPulse = VITAL_PTS_RECOVERED_PER_SEC * PULSE_TIME;

        /*BLOOD*/
        const float BLOOD_PTS_RECOVERED_PER_MIN = 10f;
        public static float AvgBloodPerPulse = BLOOD_PTS_RECOVERED_PER_MIN / 60f * PULSE_TIME;

        /*HITPOINTS*/
        const float HP_RECOVERED_PER_MIN = 5f;
        public static float AvgHpPerPulse = HP_RECOVERED_PER_MIN / 60f * PULSE_TIME;
    }

    /// <summary>
    /// Used for setting and getting the starting levels of all attributes during character creation or 
    /// loading/saving characters to the database.
    /// </summary>
    //public struct StartingLevels
    //{
    //    // Called only during character creation or when a character is loaded.
    //    public void Set(AttributeDict dict, Dictionary<string,int> startingLevels, Action LoadAttributes)
    //    { 
    //        LoadAttributes.Invoke();
    //        foreach (var level in startingLevels)
    //            dict[level.Key].SetStartingLevel(level.Value);
    //    }

    //    // Called when saving a character to the database. 
    //    public static Dictionary<string, int> Get(AttributeDict dict)
    //    {
    //        var startingLevels = new Dictionary<string, int>();

    //        foreach (var att in dict)
    //            startingLevels.Add(att.Key, att.Value.startingLevel);
    //        return startingLevels;
    //    }
    //}

    /// <summary>
    /// A container for calculating the skill points and resultant skill bonus for attributes.
    /// </summary>
    public struct SkillBonus
    {
        const int SKILLPOINTS_PER_LEVEL = 20;

        public static int Get(Skills skills, AttributeData data)
        {
            int skillBonus = 0;
            int nextLevel = SKILLPOINTS_PER_LEVEL;
            int sp = GetSkillPoints(skills, data);

            while (sp >= nextLevel)
            {
                nextLevel += SKILLPOINTS_PER_LEVEL;
                skillBonus++;
            }
            return skillBonus;
        }

        static int GetSkillPoints(Skills skills, AttributeData data)
        {
            int skillPoints = 0;

            foreach (var skill in skills.skillset.Values)
            {
                if (skill.data.primaryAttribute == data)
                    skillPoints += skill.proficiency * 2;

                if (skill.data.secondaryAttribute == data)
                    skillPoints += skill.proficiency;
            }

            return skillPoints;
        }
    }

    /// <summary>
    /// Calculates a skill's proficiency based on its xp.
    /// </summary>
    public struct Proficiency
    {
        const float FIRST_LEVEL = 100f; // Level 1 is earned at 100xp 
        const float LEVEL_MULTIPLIER = 1.02f; // Subsequent level requirements grow at a rate of 2%.

        public float nextLevelAt;

        public int Get(float xp)
        {
            int proficiency = 0;
            var requiredXp = nextLevelAt = FIRST_LEVEL;

            while (xp >= nextLevelAt)
            {
                requiredXp *= LEVEL_MULTIPLIER;
                nextLevelAt += requiredXp;
                proficiency++;
            }
            return proficiency;
        }
    }

    public struct Math
    {
        public static float Average(params float[] nums)
        {
            int i = 0;
            float total = 0;

            while (i < nums.Length)
            {
                total += nums[i];
                i++;
            }
            return total / i;
        }
    }
}

public static class D20
{
    private static Random random = new Random();
    public static int Roll() => random.Next(1, 21);
}

public static class D100
{
    private static Random random = new Random();
    public static int Roll() => random.Next(1, 101);
}
