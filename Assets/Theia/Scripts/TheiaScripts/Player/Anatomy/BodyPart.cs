using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Theia.IoC;
using UnityEngine;
using Theia.Stats.vitals;

// TODO: Setup iArmorConsumer
namespace Theia.Stats.anatomy
{
    [HideReferenceObjectPicker]
    public class BodyPart : VitalBase<BodyPartData>, iBodyPartProvider, iBodyPartConsumer, iArmorConsumer
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

        [ShowInInspector, ReadOnly]
        public int damageReduction { get; private set; }

        public void Damage(int amt = 5)
        {
            level -= amt - damageReduction;
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

        private IntProviders armorSlots = new IntProviders();
        public void Subscribe(iArmorProvider provider)
        {
            provider.AddConsumer(this);
            Notify(provider);
        }

        public void Notify(iArmorProvider provider)
        {
            armorSlots.Update(provider.GetData(), provider.GetDamageReduction(data));
            damageReduction = armorSlots.GetTotal();
        }

        // PROVIDER INTERFACE
        private BodyPartConsumers children = new BodyPartConsumers();
        public void AddConsumer(iBodyPartConsumer consumer) => children.Add(consumer);
        public bool GetCrippled() => crippled;
        public int GetLevel() => level;

    }
}
