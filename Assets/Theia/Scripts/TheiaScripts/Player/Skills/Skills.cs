using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Stats.IoC;

namespace Stats.SkillTypes
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
