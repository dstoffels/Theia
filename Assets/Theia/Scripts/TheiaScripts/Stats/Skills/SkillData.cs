﻿// A scriptable object that contains the data for all skills and acts as the key for an entity's skillset
using Abilities;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Stats
{
    [CreateAssetMenu(menuName = "Skills/Skill")]
    public class SkillData : StatData
    {
        public AttributeData primaryAttribute;
        public AttributeData secondaryAttribute;
        public Domain domain;
        [ListDrawerSettings(Expanded = true)]
        public List<Ability> abilities;
    }
}
