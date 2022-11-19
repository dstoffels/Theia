using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Stats.Values;

namespace Stats
{
    public class Vitals : StatManager<NewVital, VitalData>, iStatConsumerManager<AttributeData>
    {
        public void Init(Attributes attributes)
        {
            InitializeTemplate();
            foreach (var vital in all)
            {
                
            }
        }

        public void SubscribeAll(iStatProviderManager<AttributeData> providers)
        {
            foreach (var vital in all)
                foreach (var att in providers.Get())
                    vital.Subscribe(att);
        }
    }
}
