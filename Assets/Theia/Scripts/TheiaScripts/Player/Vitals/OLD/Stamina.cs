using UnityEngine;
using Stats.Values;
using Stats;

namespace StatsOLD
{
    [DisallowMultipleComponent]
    public class Stamina : Vital
    {
        public override float level
        {
            get { return _current; }
            set { _current = Mathf.Clamp(value, min, max); StartRecovery(); }
        }

        public override float max => att.constitution + (att.strength - Global.Attribute.AVERAGE) + (att.agility - Global.Attribute.AVERAGE) + (att.dexterity - Global.Attribute.AVERAGE);

        public override float min => -max;

        protected override float threshold => 0;

        public override float debility => Mathf.Abs(Mathf.Min(0, level));

        protected override float pointsPerPulse => att.constitution / Global.Attribute.AVERAGE * Recovery.AvgVitalPtsPerPulse;
    }
}