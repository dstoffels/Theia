using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Stats.Values;

namespace Stats
{
    public interface ISkillBuff { } // fixme: sort out stat buffs, look at uMMORPG methods

    [HideReferenceObjectPicker]
    public class Skill : BaseStat<SkillData>, iStat, iStatConsumer<AttributeData>, iStatProvider<SkillData>
    {
        [ShowInInspector, ReadOnly]
        public int level { get; private set; }

        private void SetLevel()
        {
            level = aptitude + proficiency;
            NotifyObservers();
        }
        

        // Aptitude is derived from a skill's parent attributes (primary & secondary). 
        public int aptitude { get; private set; }

        private void SetAptitude()
        {
            aptitude = 0;
            foreach (var provider in providers)
            {
                var statValue = provider.GetStatValue();
                aptitude += statValue.data == data.primaryAttribute ? 
                    statValue.value : 
                    statValue.data == data.secondaryAttribute ? 
                    statValue.value / 2 : 
                    0;
            }
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

        public void Subscribe(iStatProvider<AttributeData> provider)
        {
            var stat = provider.GetStatValue();
            if (data.Contains(stat.data)) provider.AddConsumer(this);
            Update(provider);
        }

        private List<iStatProvider<AttributeData>> providers = new List<iStatProvider<AttributeData>>();
        public void Update(iStatProvider<AttributeData> provider)
        {
            if (provider != null && !providers.Contains(provider)) providers.Add(provider);
            SetAptitude();
        }

        private List<iStatConsumer<SkillData>> observers = new List<iStatConsumer<SkillData>>();
        public void AddConsumer(iStatConsumer<SkillData> observer) { if (!observers.Contains(observer)) observers.Add(observer); }

        public void NotifyObservers() { foreach (var observer in observers) observer.Update(this); }
        public StatValue<SkillData> GetStatValue() => new StatValue<SkillData>(data, proficiency);

    }
}