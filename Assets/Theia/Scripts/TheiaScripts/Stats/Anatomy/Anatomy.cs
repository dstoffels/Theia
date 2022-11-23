using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;


namespace Stats.Anatomy
{
    public class Anatomy : StatManager<BodyPart, BodyPartData>, iStatConsumerManager<AttributeData>
    {
        public void SubscribeAll(iStatProviderManager<AttributeData> providers)
        {
            foreach (var bodypart in all)
                foreach (var att in providers.Get())
                    bodypart.Subscribe(att);

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
            foreach(var bodypart in all)
                StartCoroutine(bodypart.Recover());
        }
    }
}
