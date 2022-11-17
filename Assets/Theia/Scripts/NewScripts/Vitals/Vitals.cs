using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Stats
{
    public class Vitals : StatManager<Vital, VitalData>
    {
        protected override void GenerateStatFromData(VitalData data) => Add(data.name, new Vital(data));

        private void OnEnable()
        {
            foreach (var vital in Values) StartCoroutine(vital.Recover());
        }
    }
}
