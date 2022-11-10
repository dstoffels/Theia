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
        static int FIRST_BONUS_AT = 20;

        [ShowInInspector]
        int baseLevel;
        int raceModifier;

        Dictionary<string, int> skillValues = new Dictionary<string, int>();
        SkillPoints skillPoints = new SkillPoints();
        int nextBonusAt = FIRST_BONUS_AT;
        int lastBonusAt = 0;
        int skillBonus = 0;

        public void Update(StatEvent statEvent)
        {
            skillPoints.Set(statEvent);
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

        bool NeedsUpdate() => skillPoints.total >= nextBonusAt || skillPoints.total < lastBonusAt;

        void SetLevel()
        {
            level = baseLevel + raceModifier + skillBonus;
            NotifyDependents();
        }

        public void NotifyDependents()
        {
            foreach (var observer in observers) observer.Update(new StatEvent(name, level));
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

    struct SkillPoints
    {
        public int total => Get();
        private Dictionary<string, int> skillValues;

        int Get()
        {
            if (skillValues == null) skillValues = new Dictionary<string, int>();
            int total = 0;
            foreach (var skillValue in skillValues.Values) total += skillValue;
            return total;
        }

        public void Set(StatEvent statEvent)
        {
            if (skillValues == null) skillValues = new Dictionary<string, int>();
            if (!skillValues.ContainsKey(statEvent.name)) skillValues.Add(statEvent.name, statEvent.value);
            skillValues[statEvent.name] = statEvent.value;
        }
    }
}
