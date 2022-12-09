using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Theia.IoC;

namespace Theia.Stats.vitals
{
    public class Vitals : DataClientManager<VitalData, Vital>, iConsumerManager<iAttributeProvider>
    {
        protected override string assetPath => "Player/Vitals";
        public float impairment
        {
            get
            {
                float total = 0;
                foreach (var vital in all) total += vital.impairment;
                return total;
            }
        }

        public void SubscribeAll(iProviderManager<iAttributeProvider> providerManager)
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
