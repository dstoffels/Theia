using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;

namespace Stats
{
    [HideReferenceObjectPicker]
    public abstract class VitalBase<TVitalData> : ProviderStat<TVitalData, AttributeData>, iVital where TVitalData : VitalData
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
        public int impairment => _data.GetImpairment(this);

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

        public override void Update(StatValue<AttributeData> providerValue)
        {
            base.Update(providerValue);
            max = _data.GetMax(providerValues);
            min = _data.GetMin(this);
            threshold = _data.GetThreshold(this);
            recoveryRate= _data.GetRecoveryRate(this);
        }
        public override StatValue<TVitalData> GetStatValue() => new StatValue<TVitalData>(_data, level);
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
