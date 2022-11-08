using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;
using StatsOLD.Values;

namespace StatsOLD
{
    [DisallowMultipleComponent]
    public class Mana : Vital
    {
        public override float current
        {
            get { return _current; }
            set { _current = Mathf.Clamp(value, min, max); StartRecovery(); }
        }

        public override float max => att.discipline + (att.acuity - Global.Attribute.AVERAGE) + (att.intellect - Global.Attribute.AVERAGE) + (att.ardor - Global.Attribute.AVERAGE);

        public override float min => -max;

        protected override float threshold => 0;

        public override float debility => Mathf.Abs(Mathf.Min(0, current));

        protected override float pointsPerPulse => (att.discipline / Global.Attribute.AVERAGE) * Recovery.AvgVitalPtsPerPulse;
    }
}