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
        public bool crippled { get; private set; }
        private void checkCrippled()
        {
            crippled = level == min || parentIsCrippled;
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
                parentIsCrippled = provider.GetState(this);
            }
        }

        public void Update(iBodyPartProvider provider)
        {
            parentIsCrippled = provider.GetState(this);
            checkCrippled();
        }

        // PROVIDER INTERFACE
        private BodyPartConsumers children = new BodyPartConsumers();
        public void AddConsumer(iBodyPartConsumer consumer) => children.Add(consumer);
        public bool GetState(iBodyPartConsumer consumer) => crippled;
    }
}
