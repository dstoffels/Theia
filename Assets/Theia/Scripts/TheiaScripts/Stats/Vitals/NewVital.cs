using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Stats.Values;

namespace Stats
{
    [HideReferenceObjectPicker]
    public class NewVital : BaseStat<VitalData>, iStatConsumer
    {
        [ShowInInspector, ReadOnly]
        public float level { get; set; }

        public int debility => throw new NotImplementedException();

        [ShowInInspector, ReadOnly]
        public int max { get; private set; }
        private void SetMax()
        {
            max = 0;
            foreach (var att in data.secondaryAttributes) max += attributes.GetLevel(att) / 2;
            max += attributes.GetLevel(data.primaryAttribute);
        }

        public int min => throw new NotImplementedException();
        private iAttributeProvider attributes;

        public void AddProvider(iAttributeProvider provider)
        {
            attributes = provider;
            Update();
        }

        public override void Update()
        {
            SetMax();
        }

    }
}
