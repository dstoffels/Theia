using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Theia.IoC;
using Theia.Stats.attributes;

namespace Theia.Stats.skills
{

    [RequireComponent(typeof(Attributes)), DisallowMultipleComponent]
    public class Skills : DataClientManager<SkillData, Skill>, iConsumerManager<iAttributeProvider>, iProviderManager<iSkillProvider>
    {
        public iSkillProvider[] GetProviders() => all;
        public void SubscribeAll(iProviderManager<iAttributeProvider> providerManager)
        {
            foreach (var skill in all)
                foreach (var att in providerManager.GetProviders())
                    skill.Subscribe(att);
        }
    }

}
