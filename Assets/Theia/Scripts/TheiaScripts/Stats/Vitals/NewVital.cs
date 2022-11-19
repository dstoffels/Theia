using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Stats.Values;

namespace Stats
{
    [HideReferenceObjectPicker]
    public class NewVital : BaseStat<VitalData>, iStatConsumer<AttributeData>
    {
        [ShowInInspector, ReadOnly]
        public float level { get; set; }

        public int debility => throw new NotImplementedException();

        [ShowInInspector, ReadOnly]
        public int max { get; private set; }
        private void SetMax()
        {
            max = 0;
            foreach (var provider in providers)
            {
                var stat = provider.GetStatValue();
                max += stat.data == data.primaryAttribute ? stat.value * 2 : data.secondaryAttributes.Contains(stat.data) ? stat.value : 0;
            }
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

        public int min => throw new NotImplementedException();
    }
}
