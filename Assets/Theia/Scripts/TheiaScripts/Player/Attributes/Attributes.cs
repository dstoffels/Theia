using UnityEngine;
using Sirenix.OdinInspector;
using Theia.IoC;
using Theia.Stats.skills;

namespace Theia.Stats.attributes
{
    [RequireComponent(typeof(Skills)), DisallowMultipleComponent]
    public class Attributes : DataClientManager<AttributeData, Attribute>, iProviderManager<iAttributeProvider>, iConsumerManager<iSkillProvider>
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