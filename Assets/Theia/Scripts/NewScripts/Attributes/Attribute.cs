using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

namespace Stats
{
    [HideReferenceObjectPicker]
    public class Attribute : Stat<AttributeData>, IStatSubject, IStatObserver
    {

        int baseLevel;
        int raceModifier;

        StatValues skillPoints = new StatValues();

        const int FIRST_BONUS_AT = 10;
        int nextBonusAt = FIRST_BONUS_AT;
        int lastBonusAt = 0;
        int skillBonus = 0;

        public void Update(StatValue statValue)
        {
            skillPoints.Add(statValue);
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

        bool NeedsUpdate() => skillPoints.Total >= nextBonusAt || skillPoints.Total < lastBonusAt;

        void SetLevel()
        {
            level = baseLevel + raceModifier + skillBonus;
            NotifyDependents();
        }

        public void NotifyDependents()
        {
            foreach (var observer in observers) observer.Update(new StatValue(name, level));
        }

        public void Attach(IStatObserver observer) => observers.Add(observer);

        public void Detach(IStatObserver observer) => observers.Remove(observer);

        public Attribute(AttributeData data, int baseLevel = 10, int raceModifier=0) : base(data)
        {
            this.baseLevel = baseLevel;
            this.raceModifier = raceModifier;
            SetLevel();
        }
    }

    interface iLevel
    {
        float level { get; }
    }
}
