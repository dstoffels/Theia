using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Stats.Values;

namespace Stats
{
    public class Vitals : StatManager<NewVital, VitalData>, iStatConsumerManager
    {
        public void Init(iAttributeProvider attributeProvider)
        {
            InitializeTemplate();
            foreach (var vital in all) vital.AddProvider(attributeProvider);
        }
    }
}
