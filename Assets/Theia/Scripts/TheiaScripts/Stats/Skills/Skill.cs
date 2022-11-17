using UnityEngine;
using Sirenix.OdinInspector;
using Stats.Values;

namespace Stats
{
    public interface ISkillBuff { } // fixme: sort out stat buffs, look at uMMORPG methods

    [HideReferenceObjectPicker]
    public class Skill : BaseStat<SkillData>, iStat
    {
        [ShowInInspector]
        public int level => skillBase + proficiency;

        // The skill base is derived from a skill's parent attributes (primary & secondary).
        private int skillBase;

        // Called by the skill's attributes whenever they level up or the character is created/loaded.
        public void SetSkillBase()
        {
            skillBase = primary.level + Mathf.FloorToInt(secondary.level / 2);
        }

        // Players earn xp in a skill by using constituent abilities, leveling up the skill's proficiency at exponential increments.
        //[ShowInInspector, ReadOnly]
        private float xp;

        public void AddXp(float amount)
        {
            xp += amount;
            xp = Mathf.Max(0, xp);

            // Trigger the skill's attributes to check for a levelup
            //primary.CalculateSkillBonus();
            //secondary.CalculateSkillBonus();
        }

        public void Update()
        {
            throw new System.NotImplementedException();
        }

        private Proficiency _proficiency;
        public int proficiency => _proficiency.Get(xp);

        /*REFERENCES & WRAPPERS*/
        [HideInInspector] public SkillData data;
        [HideInInspector] public Attributes attributes;
        Attribute primary => attributes[data.primaryAttribute.name];
        Attribute secondary => attributes[data.secondaryAttribute.name];

        //public Skill(SkillData data, Attributes attributes)
        //{
        //    this.data = data;
        //    this.attributes = attributes;
        //}
    }
}