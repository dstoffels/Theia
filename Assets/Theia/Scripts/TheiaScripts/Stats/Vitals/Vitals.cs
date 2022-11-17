using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Stats.Values;

namespace Stats
{
    class Vitals : StatManager<NewVital, VitalData>
    {
        public void Init(iAttributeProvider attributeProvider)
        {
            InitializeTemplate();
            foreach (var vital in all) vital.SetProvider(attributeProvider);
        }
    }
}
