using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Stats.IoC;
using UnityEngine;

// TODO: Setup iArmorConsumer
namespace Stats.Anatomy
{
    [HideReferenceObjectPicker]
    public class BodyPart : VitalBase<BodyPartData>, iBodyPartProvider, iBodyPartConsumer
    {
        public int vulnerability { get; private set; }
        public int SetVulnerability(int accumulator) => vulnerability = data.vulnerability + accumulator;

        [ShowInInspector]
        public bool crippled { get; private set; }
        private void checkCrippled()
        {
            crippled = level == min || parentIsCrippled;
            recovering = crippled ? false : recovering;
            children.Notify(this);
        }

        public int armorCoverage { get; private set; } // TODO: wire up consumer interface

        public void Damage(int amt = 5)
        {
            level -= amt; // TODO: amt - armorCoverage
            recovering = level >= 0;
            checkCrippled();
        }

        // CONSUMER INTERFACE
        private bool parentIsCrippled;
        public void Subscribe(iBodyPartProvider provider)
        {
            if (data.Contains(provider.GetData()))
            {
                provider.AddConsumer(this);
                parentIsCrippled = provider.GetCrippled();
            }
        }

        public void Notify(iBodyPartProvider provider)
        {
            parentIsCrippled = provider.GetCrippled();
            checkCrippled();
        }

        // PROVIDER INTERFACE
        private BodyPartConsumers children = new BodyPartConsumers();
        public void AddConsumer(iBodyPartConsumer consumer) => children.Add(consumer);
        public bool GetCrippled() => crippled;
        public int GetLevel() => level;
    }
}
