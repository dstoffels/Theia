using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;


namespace Stats
{
    [HideReferenceObjectPicker]
    public class Skill : Stat<SkillData>, IStatObserver, IStatSubject
    {
        StatValues attributeValues = new StatValues();
        public int aptitude { get; private set; }
        void SetAptitude(StatValue statValue)
        {
            attributeValues.Add(statValue);
            aptitude = 0;
            foreach (var att in attributeValues)
                aptitude += Mathf.FloorToInt(
                    att.Key == data.primaryAttribute.name ? att.Value :
                    att.Key == data.secondaryAttribute.name ? att.Value / 2 : 0
                    );
            SetLevel();
        }

        [ShowInInspector, ReadOnly]
        public int xp { get; private set; }
        [Button(Style =ButtonStyle.FoldoutButton)]
        public void AddXP(int amt)
        {
            xp = Math.Max(0, xp + amt);
            SetProficiency();
        }

        const int FIRST_LEVELUP_AT = 1000;
        const float LEVELUP_MULTIPLIER = 1.02f;

        float nextLevelupAt = FIRST_LEVELUP_AT;
        float requiredXp = FIRST_LEVELUP_AT;
        float lastLevelupAt = 0;
        public int proficiency { get; private set; }

        void SetProficiency()
        {
            if (NeedsUpdate())
            {
                proficiency = 0;
                lastLevelupAt = 0;
                nextLevelupAt = FIRST_LEVELUP_AT;
                requiredXp = FIRST_LEVELUP_AT;

                while (NeedsUpdate())
                {
                    requiredXp *= LEVELUP_MULTIPLIER;
                    lastLevelupAt = nextLevelupAt;
                    nextLevelupAt += requiredXp;
                    proficiency++;
                }
                SetLevel();
                NotifyDependents();
            }
        }

        bool NeedsUpdate() => xp >= nextLevelupAt || xp < lastLevelupAt;

        void SetLevel()
        {
            level = aptitude + proficiency;
        }

        public void Update(StatValue statEvent)
        {
            SetAptitude(statEvent);
        }

        public void Attach(IStatObserver observer) => observers.Add(observer);

        public void Detach(IStatObserver observer) => observers.Remove(observer);

        public void NotifyDependents()
        {
            foreach (var observer in observers)
            {
                int value = observer.name == data.primaryAttribute.name ? proficiency * 2 : proficiency;
                observer.Update(new StatValue(name, value));
            }
        }

        public Skill(SkillData data) : base(data) {
        }

    }
}
