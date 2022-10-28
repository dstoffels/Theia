using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;
using Stats.Values;

namespace Stats
{
    [RequireComponent(typeof(Anatomy), typeof(Attributes))]
    public class HitPoints : SerializedMonoBehaviour
    {
        /*MIN/MAX*/
        [ShowInInspector]
        public float max => Math.Average(att.strength, att.constitution) / 2;

        [HideInInspector]
        public readonly float min = 0;

        /*DEBILITATION*/
        const float MAX_DISCIPLINE = 150f;
        const float MIN_DEBILITATION = 0.1f; // Minimum possible debilitation (%)

        public float debilityThreshold => max * Mathf.Max(MIN_DEBILITATION, (MAX_DISCIPLINE - att.discipline) / MAX_DISCIPLINE);

        /*RECOVERY*/
        public float recoveredPerPulse => att.constitution / Global.Attribute.AVERAGE * Recovery.AvgHpPerPulse;

        // automatic Attribute getter
        Attributes _att;
        Attributes att => _att ?? (_att = GetComponent<Attributes>()); 
    }
}