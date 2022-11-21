using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;
using Stats.Values;
using Stats;


namespace StatsOLD
{
    //[RequireComponent(typeof(Anatomy))
    [DisallowMultipleComponent]
    public class Blood : Vital
    {
        public override float level
        {
            get { return _current; }
            set { _current = Mathf.Clamp(value, min, max); StartRecovery(); }
        }

        public override float max => Math.Average(att.strength, att.constitution);

        public override float min => 0;

        protected override float threshold => max / 2;

        public override float debility => Mathf.Max(0, threshold - level);

        [ShowInInspector]
        protected override float pointsPerPulse => att.constitution / Global.Attribute.AVERAGE * Recovery.AvgBloodPerPulse;

        /*BLEEDING & RECOVERY*/

        protected override IEnumerator Recover()
        {
            var pulse = new WaitForSecondsRealtime(Recovery.PULSE_TIME);
            isRecovering = true;

            while(level != max)
            {
                level += pointsPerPulse + anatomy.totalBloodLoss;
                yield return pulse;
            }

            isRecovering = false;
        }

        Anatomy _anatomy;
        Anatomy anatomy => _anatomy ?? (_anatomy = GetComponent<Anatomy>());
    }
}