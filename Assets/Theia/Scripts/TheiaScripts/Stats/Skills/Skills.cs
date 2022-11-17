using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Stats
{
    [RequireComponent(typeof(Attributes)), DisallowMultipleComponent]
    public class Skills : StatManager<Skill, SkillData>, iAttributeProvider
    {
        public int GetSkillPoints(AttributeData att)
        {
            int total = 0;
            foreach (var skill in stats.Values)
                total += skill.data.primaryAttribute == att ? skill.level * 2 : skill.data.secondaryAttribute == att ? skill.level : 0;
            return total;
        }
    }

    public interface iAttributeProvider
    {
        int GetSkillPoints(AttributeData att);
    }
}
