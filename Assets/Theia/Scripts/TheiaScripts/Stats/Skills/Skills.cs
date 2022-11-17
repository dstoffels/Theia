using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Stats
{
    [RequireComponent(typeof(Attributes)), DisallowMultipleComponent]
    public class Skills : StatManager<Skill, SkillData>, iSkillProvider
    {
        public int GetSkillPoints(AttributeData att)
        {
            int total = 0;
            foreach (var skill in all)
                total += skill.data.primaryAttribute == att ? skill.proficiency * 2 : skill.data.secondaryAttribute == att ? skill.proficiency : 0;
            return total;
        }

        public void NotifyDependents(BaseData statData)
        {
            foreach (var skill in all)
                if (skill.data.primaryAttribute == statData || skill.data.secondaryAttribute == statData)
                    skill.Update();
        }
        public void Init(iAttributeProvider attributeProvider)
        {
            InitializeTemplate();
            foreach (var skill in all) skill.SetProvider(attributeProvider);
        }
    }


    public interface iSkillProvider : iProvider
    {
        int GetSkillPoints(AttributeData att);
    }
}
