using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Stats.IoC;

namespace Stats
{

    [RequireComponent(typeof(Attributes)), DisallowMultipleComponent]
    public class Skills : StatManager<Skill, SkillData>, iConsumerManager<int>, iProviderManager<int>
    {
        public iProvider<int>[] GetProviders() => all;
        public void SubscribeAll(iProviderManager<int> providerManager)
        {
            foreach (var skill in all)
                foreach (var att in providerManager.GetProviders())
                    skill.Subscribe(att);
        }
    }

}
