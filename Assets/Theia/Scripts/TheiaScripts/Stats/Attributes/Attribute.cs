using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Stats.Values;

namespace Stats
{
    // TODO: Implement interfaces to decouple
    public interface iStatBuff { } // fixme: sort out stat buffs, look at uMMORPG methods

    [HideReferenceObjectPicker]
    public class Attribute : BaseStat<AttributeData>, iStat
    {
        [ShowInInspector, ReadOnly]
        public int level { get; private set; }
        private void SetLevel() => level = startingLevel + raceModifier + skillBonus;

        //starting level is intialized at character creation, customized manually and modified by species
        public int startingLevel { get; private set; } = 10;
            
        public int raceModifier { get; private set; } = 0;

        // The skill bonus is how an attribute grows. Skill levels are derived from two attributes each (skill base).
        // A skill's parent attributes earn skill points each time it levels up (20 skill points per level)
        public int skillBonus { get; private set; }

        private static readonly int FIRST_BONUS_AT = 10;
        private int nextBonusAt = FIRST_BONUS_AT;
        private int lastBonusAt = 0;
        public int skillPoints { get; private set; }
        private bool needsUpdate => skillPoints >= nextBonusAt || skillPoints < lastBonusAt;

        public void Update()
        {
            if (needsUpdate)
            {
                skillBonus = 0;
                nextBonusAt = FIRST_BONUS_AT;
                lastBonusAt = 0;

                while (needsUpdate)
                {
                    lastBonusAt = nextBonusAt;
                    nextBonusAt += FIRST_BONUS_AT;
                    skillBonus++;
                }
                SetLevel();
            }
        }

        public void Load(int startingLevel, int raceModifier)
        {
            this.startingLevel = startingLevel;
            this.raceModifier = raceModifier;
            SetLevel();
        }

        public override void Init(AttributeData data)
        {
            base.Init(data);
            SetLevel();
        }
    }
}