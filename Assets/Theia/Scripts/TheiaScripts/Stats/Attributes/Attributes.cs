using Stats.Values;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

namespace Stats
{
    /// <summary>
    /// Provides an interface to the attribute stat manager to retrieve values using AttributeData.
    /// </summary>
    public interface iAttributeProvider : iStatProvider { }

    [RequireComponent(typeof(Skills)), DisallowMultipleComponent]
    public class Attributes : StatManager<Attribute, AttributeData>, iAttributeProvider
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

        public int GetLevel(BaseData stat) => this[stat.name].level;
        public void NotifyConsumers(BaseData statData)
        {
            foreach (var att in all)
                if (att.data == statData) 
                    att.Update();
        }

        public void Init(iSkillProvider skillProvider)
        {
            InitializeTemplate();
            foreach (var att in all) att.SetProvider(skillProvider);
        }
    } 

}