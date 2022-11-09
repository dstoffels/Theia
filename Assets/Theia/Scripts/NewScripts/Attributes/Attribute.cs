using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

namespace Stats
{
    [Serializable]
    public class Attribute : Stat<AttributeData>, IStatSubject, IStatObserver
    {
        static int FIRST_BONUS_AT = 20;

        int baseLevel = 10;
        int raceModifier = 0;

        Dictionary<string, int> skillValues = new Dictionary<string, int>();
        int skillPoints = 0;
        int nextBonusAt = FIRST_BONUS_AT;
        int lastBonusAt = 0;
        int skillBonus = 0;

        public void Update(StatEvent statEvent)
        {
            SetSkillPoints(statEvent);
            SetSkillBonus();
        }

        private void SetSkillBonus()
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
                SetLevel();
            }
        }

        bool NeedsUpdate() => skillPoints >= nextBonusAt || skillPoints < lastBonusAt;

        void SetLevel()
        {
            level = baseLevel + raceModifier + skillBonus;
            NotifyDependents();
        }

        void SetSkillPoints(StatEvent statEvent)
        {
            if (!skillValues.ContainsKey(statEvent.name)) skillValues.Add(statEvent.name, statEvent.value);
            skillValues[statEvent.name] = statEvent.value;
            skillPoints = 0;
            foreach (var skillValue in skillValues.Values) skillPoints += skillValue;
        }

        public void NotifyDependents()
        {
            foreach (var observer in observers) observer.Update(new StatEvent(name, level));
        }

        public void Attach(IStatObserver observer) => observers.Add(observer);

        public void Detach(IStatObserver observer) => observers.Remove(observer);

        public Attribute(AttributeData data) : base(data) { }
    }
}
