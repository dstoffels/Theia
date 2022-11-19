using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Stats
{

    [RequireComponent(typeof(Attributes)), DisallowMultipleComponent]
    public class Skills : StatManager<Skill, SkillData>, iStatConsumerManager<AttributeData>, iStatProviderManager<SkillData>
    {
        public iStatProvider<SkillData>[] Get() => all;

        public void SubscribeAll(iStatProviderManager<AttributeData> providers)
        {
            if(!initialized)
                foreach (var skill in all)
                    foreach (var att in providers.Get())
                        skill.Subscribe(att);
            initialized = true;
        }
    }

}
