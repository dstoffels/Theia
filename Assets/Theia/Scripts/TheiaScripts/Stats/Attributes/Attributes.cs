using Stats.Values;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

namespace Stats
{
    [RequireComponent(typeof(Skills)), DisallowMultipleComponent]
    public class Attributes : StatManager<Attribute, AttributeData>, iSkillProvider
    {

        // Accessors //
        public int strength => this["Strength"].level;
        public int constitution => this["Strength"].level;
        public int agility => this["Agility"].level;
        public int dexterity => this["Dexterity"].level;
        public int acuity => this["Acuity"].level;
        public int intellect => this["Intellect"].level;
        public int discipline => this["Discipline"].level;
        public int ardor => this["Ardor"].level;
        public int GetAptitude(AttributeData pri, AttributeData sec) => this[pri].level + this[sec].level / 2;
    }


    public interface iSkillProvider
    {
        int GetAptitude(AttributeData pri, AttributeData sec);
    }
}