using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using Stats.IoC;

namespace Stats
{
    [HideReferenceObjectPicker]
    public abstract class VitalBase<TVitalData> : BaseStat<TVitalData>, iVital, iConsumer<int> where TVitalData : VitalData
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
        [ShowInInspector, ReadOnly]
        public int threshold { get; private set; }
        [ShowInInspector, ReadOnly]
        public int impairment => data.GetImpairment(this);

        // RECOVERY
        [ShowInInspector]
        public bool isRecovering { get; set; } = true;
        public int recoveryRate { get; private set; }

        public IEnumerator Recover()
        {
            while (true)
            {
                if(isRecovering) level++;
                yield return new WaitForSeconds(recoveryRate / 1000);
            }
        }

        // CONSUMER INTERFACE
        private Providers<int> providers = new Providers<int>();
        public void Subscribe(iProvider<int> provider)
        {
            if (data.Contains(provider.GetData()))
            {
                provider.AddConsumer(this);
                Update(provider);
            }
        }

        public void Update(iProvider<int> provider)
        {
            providers.Update(provider, this);
            max = data.GetMax(providers);
            min = data.GetMin(this);
            threshold = data.GetThreshold(this);
            recoveryRate= data.GetRecoveryRate(this);
        }
        public BaseData GetData() => data;
    }

    public interface iVital
    {
        int level { get; }
        int max { get; }
        int min { get; }
        int threshold { get; }
        int impairment { get; }
        bool isRecovering { get; }
        int recoveryRate { get; }
        IEnumerator Recover();
    }
}
