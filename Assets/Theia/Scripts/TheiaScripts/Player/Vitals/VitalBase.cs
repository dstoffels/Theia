using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using Theia.IoC;

namespace Theia.Stats.vitals
{
    [HideReferenceObjectPicker]
    public abstract class VitalBase<TVitalData> : DataClient<TVitalData>, iVital, iAttributeConsumer where TVitalData : VitalData
    {
        protected int _level;
        [ShowInInspector, ReadOnly]
        public int level
        {
            get => _level;
            protected set => _level = Mathf.Clamp(value, min, max);
        }

        [ShowInInspector, ReadOnly]
        public int max { get; private set; }
        public int min { get; private set; }
        public int threshold { get; private set; }
        [ShowInInspector, ReadOnly]
        public int impairment => data.GetImpairment(this);

        // RECOVERY
        [ShowInInspector]
        public bool recovering { get; set; } = true;
        public int recoveryRate { get; private set; }

        public IEnumerator Recover()
        {
            while (true)
            {
                yield return new WaitForSeconds(recoveryRate / 1000);
                if(recovering) level++;
            }
        }

        // CONSUMER INTERFACE
        private IntProviders attributes = new IntProviders();

        public void Subscribe(iAttributeProvider provider)
        {
            if (data.Contains(provider.GetData()))
            {
                provider.AddConsumer(this);
                Notify(provider);
            }
        }

        public void Notify(iAttributeProvider provider)
        {
            attributes.Update(provider.GetData(), provider.GetLevel());
            max = data.GetMax(attributes);
            min = data.GetMin(this);
            threshold = data.GetThreshold(this);
            recoveryRate = data.GetRecoveryRate(this);
        }
        public BaseData GetData() => data;
    }
}
