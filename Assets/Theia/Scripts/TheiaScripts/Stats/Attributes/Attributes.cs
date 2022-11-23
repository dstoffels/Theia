using Stats.Values;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using Stats.IoC;

namespace Stats
{

    [RequireComponent(typeof(Skills)), DisallowMultipleComponent]
    public class Attributes : StatManager<Attribute, AttributeData>, iProviderManager<int>, iConsumerManager<int>
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

        iProvider<int>[] iProviderManager<int>.GetProviders() => all;

        public void SubscribeAll(iProviderManager<int> providerManager)
        {
            foreach (var att in all)
                foreach (var skill in providerManager.GetProviders())
                    att.Subscribe(skill);
        }
    } 
}