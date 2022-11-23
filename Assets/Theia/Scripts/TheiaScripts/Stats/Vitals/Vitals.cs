using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Stats.Values;
using Stats.IoC;
using UnityEditor.VersionControl;

namespace Stats
{
    public class Vitals : StatManager<Vital, VitalData>, iConsumerManager<int>
    {
        public float impairment
        {
            get
            {
                float total = 0;
                foreach (var vital in all) total += vital.impairment;
                return total;
            }
        }

        public void SubscribeAll(iProviderManager<int> providerManager)
        {
            foreach (var vital in all)
                foreach (var att in providerManager.GetProviders())
                    vital.Subscribe(att);
        }
        private void Start()
        {
            foreach (var vital in all)
                StartCoroutine(vital.Recover());
        }
    }

    
}
