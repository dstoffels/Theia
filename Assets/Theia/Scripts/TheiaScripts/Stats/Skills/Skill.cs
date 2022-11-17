using UnityEngine;
using Sirenix.OdinInspector;
using Stats.Values;

namespace Stats
{
    public interface ISkillBuff { } // fixme: sort out stat buffs, look at uMMORPG methods

    [HideReferenceObjectPicker]
    public class Skill : BaseStat<SkillData>, iStat, iStatConsumer<iAttributeProvider>
    {
        [ShowInInspector, ReadOnly]
        public int level { get; private set; }

        private void SetLevel() => level = apititude + proficiency;
        

        // Aptitude is derived from a skill's parent attributes (primary & secondary). 
        public int apititude { get; private set; }

        private iAttributeProvider attributes;

        private void SetAptitude()
        {
            int pri = attributes.GetLevel(data.primaryAttribute);
            int sec = attributes.GetLevel(data.secondaryAttribute);
            apititude = pri + sec / 2;
            SetLevel();
        }

        // Called by the skill's attributes whenever they level up or the character is created/loaded.


        // Players earn xp in a skill by using constituent abilities, leveling up the skill's proficiency at exponential increments.
        //[ShowInInspector, ReadOnly]
        public int xp { get; private set; }

        [Button(Style =ButtonStyle.FoldoutButton)]
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
                attributes.NotifyConsumers(data.primaryAttribute);
                attributes.NotifyConsumers(data.secondaryAttribute);
                SetLevel();
            }
        }

        public void Update()
        {
            SetAptitude();
        }

        public void SetProvider(iAttributeProvider provider)
        {
            attributes = provider;
            Update();
        }

        public int _proficiency => Proficiency.Get(xp);

    }
}