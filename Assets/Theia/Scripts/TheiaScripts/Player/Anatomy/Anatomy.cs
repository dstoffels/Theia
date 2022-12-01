using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Theia.IoC;
using UnityEngine;


namespace Theia.Stats.anatomy
{
    public class Anatomy : DataClientManager<BodyPartData, BodyPart>, iConsumerManager<iAttributeProvider>, iConsumerManager<iArmorProvider>
    {
        public int totalVulnerability = 0;

        [ShowInInspector, ReadOnly, PropertyOrder(-1)]
        public float impairment
        {
            get
            {
                float total = 0;
                foreach (var vital in all) 
                    total += vital.impairment; 
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

        protected override void InitCallback(BodyPartData data)
        {
            base.InitCallback(data);
            totalVulnerability = 0;
            foreach (var bp in all)
                totalVulnerability = bp.SetVulnerability(totalVulnerability); 
        }

        [Button]
        public void Damage(BodyPartData bodyPart = null, int amt = 5)
        {
            if (bodyPart != null)
                this[bodyPart].Damage(amt);
            else
            {
                var die = new System.Random().Next(1, totalVulnerability + 1);
                foreach (var bp in all)
                {
                    if (die <= bp.vulnerability)
                    {
                        bp.Damage(amt);
                        break;
                    }
                }
            }
        }

        public void SubscribeAll(iProviderManager<iArmorProvider> providerManager)
        {
            foreach (var bp in all)
                foreach (var armorSlot in providerManager.GetProviders())
                    bp.Subscribe(armorSlot);
        }
    }
}
