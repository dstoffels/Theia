using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Stats
{
    [HideReferenceObjectPicker]
    public class Vital : Stat<VitalData>, IVital, IStatObserver
    {
        StatValues attributes = new StatValues();

        float _level;
        [HideInInspector]
        new public float level { get { return _level; } private set { _level = Mathf.Clamp(value, min, max); } }

        public float max { get; private set; } = 10;

        public float min => data.isFullScale ? -max : 0;

        public float threshold => data.isFullScale ? 0 : max / 2;
        public float debility => Mathf.Abs(Mathf.Min(threshold, level));

        public bool isRecovering { get; private set; } = true;
        float ptsRecoveredPerPulse => attributes[data.recoveryAttribute] / 10 * data.avgVitalPtsPerPulse;

        /// <summary>
        /// A Coroutine that must be started by parent Monobehaviour.
        /// </summary>
        public IEnumerator Recover()
        {
            var pulse = new WaitForSecondsRealtime(VitalData.RECOVERY_PULSE);
            while (level != max)
            {
                //if (isRecovering)
                level += 0.066f;
                Debug.Log(level);
                yield return pulse;
            }
        }

        public void Update(StatValue statValue)
        {
            attributes.Add(statValue);
            SetMax();
        }

        void SetMax()
        {
            float total = attributes[data.primaryAttribute];
            foreach (var stat in data.secondaryAttributes)
                total += attributes[stat] / 2;
            max = total;
        }

        public Vital(VitalData data) : base(data) { }
    }
}
