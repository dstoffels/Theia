// A scriptable object that contains the data for all skills and acts as the key for an entity's skillset
using Abilities;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Stats.ArchetypeTypes;

namespace Stats.SkillTypes
{
    [CreateAssetMenu(menuName = "Skills/Skill")]
    public class SkillData : BaseData
    {
        public AttributeData primaryAttribute;
        public AttributeData secondaryAttribute;
        public Domain domain;
        public SpecType specialization;


        public override bool Contains(BaseData stat) => stat == primaryAttribute || stat == secondaryAttribute;
    }
}
