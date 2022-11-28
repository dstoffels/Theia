using UnityEngine;
using Sirenix.OdinInspector;
using Stats.IoC;
using Stats.SkillTypes;

namespace Stats
{
    [RequireComponent(typeof(Skills)), DisallowMultipleComponent]
    public class Attributes : StatManager<Attribute, AttributeData>, iProviderManager<iAttributeProvider>, iConsumerManager<iSkillProvider>
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

        public iAttributeProvider[] GetProviders() => all;

        public void SubscribeAll(iProviderManager<iSkillProvider> providerManager)
        {
            foreach (var att in all)
                foreach (var skill in providerManager.GetProviders())
                    att.Subscribe(skill);
        }
    } 
}