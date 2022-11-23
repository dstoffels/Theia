using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Stats.Values;

namespace Stats
{
    public class Vitals : StatManager<Vital, VitalData>, iStatConsumerManager<AttributeData>
    {
        public void SubscribeAll(iStatProviderManager<AttributeData> providers)
        {
            foreach (var vital in all)
                foreach (var att in providers.Get())
                    vital.Subscribe(att);
        }

        public float impairment
        {
            get
            {
                float total = 0;
                foreach (var vital in all) total += vital.impairment;
                return total;
            }
        }

        private void Start()
        {
            foreach (var vital in all)
                StartCoroutine(vital.Recover());
        }
    }

    
}
