using System.Collections;
using UnityEngine;
using Stats.Values;
using Sirenix.OdinInspector;

namespace Stats
{
    [DisallowMultipleComponent]
    public class Temperature : Vital
    {
        public override float max => att.constitution;
        public override float min => -max;
        protected override float threshold => att.discipline / 2;

        public bool hot => current > threshold;
        public bool cold => current < -threshold;

        public override float debility
        {
            get
            {
                if (hot) 
                    return current - threshold;
                if (cold)
                    return Mathf.Abs(current + threshold);
                return 0;
            }
        }

        protected override float pointsPerPulse => att.constitution / Global.Attribute.AVERAGE * Recovery.AvgVitalPtsPerPulse;

        protected override IEnumerator Recover()
        {
            var zeroPoint = 0.25f;
            var pulse = new WaitForSecondsRealtime(Recovery.PULSE_TIME);
            isRecovering = true;

            while (current != 0)
            {
                if (current > 0) current -= pointsPerPulse;
                if (current < 0) current += pointsPerPulse;
                if (current < zeroPoint && current > -zeroPoint) current = 0;    // set level to zero to avoid bouncing back and forth.

                yield return pulse;
            }

            isRecovering = false;
        }
    }
}