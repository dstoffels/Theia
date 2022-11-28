using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Stats.IoC;
using UnityEngine;

namespace Stats.Anatomy
{
    [HideReferenceObjectPicker]
    public class BodyPart : VitalBase<BodyPartData>, iBodyPartProvider, iBodyPartConsumer
    {
        [ShowInInspector]
        public int vulnerability { get; private set; }
        public int SetVulnerability(int accumulator) => vulnerability = data.vulnerability + accumulator;

        [ShowInInspector]
        public bool crippled { get; private set; }
        private void checkCrippled()
        {
            crippled = level == min || parentIsCrippled;
            isRecovering = crippled ? false : isRecovering;
            children.Notify(this);
        }
        [Button]
        public void Damage(int amt = 5)
        {
            level -= amt;
            isRecovering = level >= 0;
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

        public void Update(iBodyPartProvider provider)
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
