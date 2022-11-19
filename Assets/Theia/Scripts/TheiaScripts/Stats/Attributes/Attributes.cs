using Stats.Values;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

namespace Stats
{

    [RequireComponent(typeof(Skills)), DisallowMultipleComponent]
    public class Attributes : StatManager<Attribute, AttributeData>, iStatProviderManager<AttributeData>, iStatConsumerManager<SkillData>
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

        public iStatProvider<AttributeData>[] Get() => all;

        public void SubscribeAll(iStatProviderManager<SkillData> providers)
        {
            foreach (var att in all)
                foreach (var skill in providers.Get())
                    att.Subscribe(skill);
        }
    } 
}