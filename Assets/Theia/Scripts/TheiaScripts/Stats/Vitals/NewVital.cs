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

        }

        // TODO: implement update and list of providers
        public void Update(iStatProvider<AttributeData> provider)
        {
            throw new NotImplementedException();
        }

        public void Subscribe(iStatProvider<AttributeData> provider)
        {
            throw new NotImplementedException();
        }

        public int min => throw new NotImplementedException();
    }
}
