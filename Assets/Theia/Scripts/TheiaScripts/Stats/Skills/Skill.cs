using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Stats.Values;

namespace Stats
{
    public interface ISkillBuff { } // fixme: sort out stat buffs, look at uMMORPG methods

    [HideReferenceObjectPicker]
    public class Skill : ProviderStat<SkillData, AttributeData>
    {
        [ShowInInspector, ReadOnly]
        public int level { get; private set; }

        private void SetLevel()
        {
            int newLevel = aptitude + proficiency;
            if (level != newLevel)
            {
                level= newLevel;
                NotifyObservers();
            }
        }
        

        // Aptitude is derived from a skill's parent attributes (primary & secondary). 
        public int aptitude { get; private set; }

        private void SetAptitude()
        {
            aptitude = providerValues.Reduce(att =>
                att.data == data.primaryAttribute ? att.value :
                att.data == data.secondaryAttribute ? att.value / 2 : 0
            );
            SetLevel();
        }

        // Players earn xp in a skill by using constituent abilities, leveling up the skill's proficiency at exponential increments.
        //[ShowInInspector, ReadOnly]
        public int xp { get; private set; }

        [Button(Style = ButtonStyle.FoldoutButton)]
        public void AddXp(int amount=5000)
        {
            xp = Mathf.Max(0, xp + amount);

            SetProficiency();
        }

        public int proficiency { get; private set; }

        private static readonly float FIRST_BONUS_AT = 1000;
        private static readonly float BONUS_MODIFIER = 1.02f;
        private float nextBonusAt, xpRequired = FIRST_BONUS_AT;
        private float lastBonusAt = 0;

        private bool needsUpdate => xp >= nextBonusAt || xp < lastBonusAt;
        private void SetProficiency()
        {
            if (needsUpdate)
            {
                lastBonusAt = proficiency = 0;
                nextBonusAt = xpRequired = FIRST_BONUS_AT;

                while (needsUpdate)
                {
                    lastBonusAt = nextBonusAt;
                    xpRequired *= BONUS_MODIFIER;
                    nextBonusAt += xpRequired;
                    proficiency++;
                }
                SetLevel();
            }
        }

        public override void Update(StatValue<AttributeData> providerValue)
        {
            base.Update(providerValue);
            SetAptitude();
        }

        public override StatValue<SkillData> GetStatValue() => new StatValue<SkillData>(data, proficiency);
    }
}