using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Entities;

namespace Stats
{
    public class Attribute : Stat
    {
        public static int FIRST_BONUS_AT = 20;

        private int baseLevel = 0;
        private int raceModifier = 0;

        private int skillPoints = 0;
        private int nextBonusAt = FIRST_BONUS_AT;
        private int lastBonusAt = 0;
        private int skillBonus = 0;


        public override void Update(StatEvent statEvent)
        {
            setSkillBonus();
            // TODO: update all dependent stats (skills, vitals)
            // set Level: baseLevel, raceMod, skillBonus
            Level = baseLevel + raceModifier + skillBonus;
        }

        private void setSkillBonus()
        {
            if (NeedsUpdate())
            {
                skillBonus = 0;
                nextBonusAt = FIRST_BONUS_AT;
                lastBonusAt = 0;

                while (NeedsUpdate())
                {
                    lastBonusAt = nextBonusAt;
                    nextBonusAt += FIRST_BONUS_AT;
                    skillBonus++;
                }
            }
        }

        protected override bool NeedsUpdate()
        {
            return skillPoints >= nextBonusAt || skillPoints < lastBonusAt;
        }
    }

    public interface IAttributeData : IStatData
    {

    }
}
