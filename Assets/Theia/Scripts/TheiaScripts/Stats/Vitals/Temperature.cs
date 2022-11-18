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

        public bool hot => level > threshold;
        public bool cold => level < -threshold;

        public override float debility
        {
            get
            {
                if (hot) 
                    return level - threshold;
                if (cold)
                    return Mathf.Abs(level + threshold);
                return 0;
            }
        }

        protected override float pointsPerPulse => att.constitution / Global.Attribute.AVERAGE * Recovery.AvgVitalPtsPerPulse;

        protected override IEnumerator Recover()
        {
            var zeroPoint = 0.25f;
            var pulse = new WaitForSecondsRealtime(Recovery.PULSE_TIME);
            isRecovering = true;

            while (level != 0)
            {
                if (level > 0) level -= pointsPerPulse;
                if (level < 0) level += pointsPerPulse;
                if (level < zeroPoint && level > -zeroPoint) level = 0;    // set level to zero to avoid bouncing back and forth.

                yield return pulse;
            }

            isRecovering = false;
        }
    }
}