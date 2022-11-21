using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Stats
{
    // TODO: figure out if the scriptable object route is the right way to go. i.e.
    // ALL logic for vitals will be stored in VitalData classes
    // Hit points can be "plugged in" to body parts.
    // Avoid having to write cases for each vitalData in the vital class, while allowing for flexibility when changes are made down the road
    // Still allows for Vitals StatManager so entity/player class doesn't have to manage.

    [HideReferenceObjectPicker]
    public class Vital : ConsumerStat<VitalData, AttributeData>
    {
        private float _level;
        [ShowInInspector, ReadOnly]
        public float level
        {
            get { return _level; }
            set { _level = Mathf.Clamp(value, min, max); } // StartRecovery();
        }

        [ShowInInspector, ReadOnly]
        public int max { get; private set; }
        public int min { get; private set; }
        public int threshold { get; private set; }
        public float debility => data.GetDebility(this);
        public override void Update(StatValue<AttributeData> providerValue)
        {
            base.Update(providerValue);
            providerValues.Add(providerValue);
            max = data.GetMax(providerValues);
            min = data.GetMin(this);
            threshold = data.GetThreshold(this);
        }
    }
}
