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
    public class Vital : BaseStat<VitalData>, iStatConsumer<AttributeData>
    {
        [ShowInInspector, ReadOnly]
        public float level { get; set; }

        [ShowInInspector, ReadOnly]
        public int max { get; private set; }
        public int min { get; private set; }
        public int threshold { get; private set; }
        public float debility => data.GetDebility(this);
        private void SetMax()
        {
            max = data.GetMax(providers);
            min = data.GetMin(this);
            threshold = data.GetThreshold(this);
            //foreach (var provider in providers)
            //{
            //    var stat = provider.GetStatValue();
            //    max += stat.data == data.primaryAttribute ? stat.value * 2 : data.secondaryAttributes.Contains(stat.data) ? stat.value : 0;
            //}
        }

        private List<iStatProvider<AttributeData>> providers = new List<iStatProvider<AttributeData>>();
        public void Update(iStatProvider<AttributeData> provider)
        {
            if (!providers.Contains(provider)) providers.Add(provider);
            SetMax();
        }

        public void Subscribe(iStatProvider<AttributeData> provider)
        {
            var stat = provider.GetStatValue();
            if (data.Contains(stat.data)) provider.AddConsumer(this);
            Update(provider);
        }

    }
}
