using UnityEngine;
using Sirenix.OdinInspector;
using Stats.Values;

namespace Stats
{
    public interface IAttributeBuff { } // fixme: sort out stat buffs, look at uMMORPG methods

    [HideReferenceObjectPicker]
    public class Attribute
    {
        [ShowInInspector]
        public int level => startingLevel + skillBonus;

        //starting level is intialized at character creation, customized manually and modified by species
        public int startingLevel { get; private set; }

        // The skill bonus is how an attribute grows. Skill levels are derived from two attributes each (skill base).
        // A skill's parent attributes earn skill points each time it levels up (20 skill points per level)
        private int skillBonus;
        
        public void CalculateSkillBonus()
        {
            skillBonus = SkillBonus.Get(skills, data);
            skills.SetSkillBases();
        }

        //executed at character creation and whenever character is loaded
        public void SetStartingLevel(int level)
        {
            startingLevel = level;
            skills.SetSkillBases();
        }

        /*REFERENCES*/
        [HideInInspector] public AttributeData data;
        [HideInInspector] public Skills skills;

        public Attribute(AttributeData data, Skills skills)
        {
            this.data = data;
            this.skills = skills;
        }
    }
}