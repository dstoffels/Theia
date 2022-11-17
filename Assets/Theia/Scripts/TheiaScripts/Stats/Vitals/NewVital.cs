using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Stats.Values;

namespace Stats
{
    public class NewVital : BaseStat<VitalData>, iStatConsumer<iAttributeProvider>
    {
        private iAttributeProvider attributes;
        public void SetProvider(iAttributeProvider provider) => attributes = provider;

        public void Update()
        {
            throw new NotImplementedException();
        }
    }
}
