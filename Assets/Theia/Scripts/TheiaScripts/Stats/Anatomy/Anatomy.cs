using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Stats.IoC;
using UnityEngine;


namespace Stats.Anatomy
{
    public class Anatomy : StatManager<BodyPart, BodyPartData>, iConsumerManager<iAttributeProvider>
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

        public void SubscribeAll(iProviderManager<iAttributeProvider> providerManager)
        {
            foreach (var bodypart in all)
                foreach (var att in providerManager.GetProviders())
                {
                    bodypart.Subscribe(att);
                    if (bodypart.data.parent)
                        bodypart.Subscribe(this[bodypart.data.parent]);
                }
        }

        private void Start()
        {
            foreach (var bodypart in all)
                StartCoroutine(bodypart.Recover());
        }

    }
}
