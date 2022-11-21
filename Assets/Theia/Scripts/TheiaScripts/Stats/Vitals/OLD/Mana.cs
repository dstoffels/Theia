using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;
using Stats.Values;
using Stats;

namespace StatsOLD
{
    [DisallowMultipleComponent]
    public class Mana : Vital
    {
        public override float level
        {
            get { return _current; }
            set { _current = Mathf.Clamp(value, min, max); StartRecovery(); }
        }

        public override float max => att.discipline + (att.acuity - Global.Attribute.AVERAGE) + (att.intellect - Global.Attribute.AVERAGE) + (att.ardor - Global.Attribute.AVERAGE);

        public override float min => -max;

        protected override float threshold => 0;

        public override float debility => Mathf.Abs(Mathf.Min(0, level));

        protected override float pointsPerPulse => (att.discipline / Global.Attribute.AVERAGE) * Recovery.AvgVitalPtsPerPulse;
    }
}