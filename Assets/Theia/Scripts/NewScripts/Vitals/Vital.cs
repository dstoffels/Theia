using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Stats
{
    public class Vital : Stat<VitalData>, IVital, IStatObserver
    {
        private StatValues attributes = new StatValues();

        new public float level { get; private set; }

        public float max { get; private set; }

        public float min => data.isFullScale ? -max : 0;

        public float threshold => data.isFullScale ? 0 : max / 2;
        public float debility => Mathf.Abs(Mathf.Min(threshold, level));

        public bool isRecovering { get; private set; }
        float ptsRecoveredPerPulse => attributes[data.recoveryAttribute] / 10 * data.avgVitalPtsPerPulse;
        public void StartRecovery()
        {
            //if (!isRecovering)
                //StartCoroutine(Recover());
        }

        public IEnumerator Recover()
        {
            var pulse = new WaitForSecondsRealtime(VitalData.RECOVERY_PULSE);
            isRecovering = true;

            while (level != max)
            {
                level += ptsRecoveredPerPulse;
                yield return pulse;
            }

            isRecovering = false;
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
