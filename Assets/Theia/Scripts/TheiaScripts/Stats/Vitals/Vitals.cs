using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Stats.Values;

namespace Stats
{
    public class Vitals : StatManager<NewVital, VitalData>
    {
        public void Init(Attributes attributes)
        {
            InitializeTemplate();
            foreach (var vital in all)
            {
                
            }
        }
    }
}
