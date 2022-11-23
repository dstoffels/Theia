using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Stats.IoC;

namespace Stats
{
    public interface ISkillBuff { } // fixme: sort out stat buffs, look at uMMORPG methods

    [HideReferenceObjectPicker]
    public class Skill : BaseStat<SkillData>, iProvider<int>, iConsumer<int>
    {
        [ShowInInspector, ReadOnly]
        public int level { get; private set; }

        private void SetLevel()
        {
            int newLevel = aptitude + proficiency;
            if (level != newLevel)
            {
                level= newLevel;
                consumers.Notify(this);
            }
        }
        

        // Aptitude is derived from a skill's parent attributes (primary & secondary). 
        public int aptitude { get; private set; }

        private void SetAptitude()
        {
            aptitude = 0;
            foreach (var att in providers)
                aptitude +=
                    data.primaryAttribute == att.Key ?
                        att.Value :
                    data.secondaryAttribute == att.Key ?
                        att.Value / 2:
                    0;
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

        //CONSUMER INTERFACE
        private Providers<int> providers = new Providers<int>();
        public void Subscribe(iProvider<int> provider)
        {
            if (data.Contains(provider.GetData()))
            {
                provider.AddConsumer(this);
                Update(provider);
            }
        }

        public void Update(iProvider<int> provider)
        {
            providers.Update(provider, this);
            SetAptitude();
        }


        // PROVIDER INTERFACE
        private Consumers<int> consumers = new Consumers<int>();
        public void AddConsumer(iConsumer<int> consumer) => consumers.Add(consumer);
        public int GetValue(iConsumer<int> consumer) =>
            data.primaryAttribute == consumer.GetData() ?
                proficiency :
            data.secondaryAttribute == consumer.GetData() ?
                proficiency / 2 :
            0;

        public BaseData GetData() => data;

    }
}