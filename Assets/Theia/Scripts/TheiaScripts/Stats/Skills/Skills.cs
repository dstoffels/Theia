using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Stats
{

    [RequireComponent(typeof(Attributes)), DisallowMultipleComponent]
    public class Skills : StatManager<Skill, SkillData>, iSkillProviderManager
    {
        public int GetSkillPoints(AttributeData att)
        {
            int total = 0;
            foreach (var skill in all)
                total += skill.data.primaryAttribute == att ? skill.proficiency * 2 : skill.data.secondaryAttribute == att ? skill.proficiency : 0;
            return total;
        }
        public int GetLevel(SkillData stat) => this[stat.name].level;

        public void Init(iAttributeProvider attributeProvider)
        {
            InitializeTemplate();
            foreach (var skill in all) skill.AddProvider(attributeProvider);
        }
    }
    public interface iSkillProviderManager : iStatProvider<SkillData>, iStatConsumerManager
    {
        int GetSkillPoints(AttributeData att);
    }

}
