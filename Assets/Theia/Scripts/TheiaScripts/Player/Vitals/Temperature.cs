using System;
using System.Collections.Generic;
using UnityEngine;

namespace Theia.Stats.vitals
{
    [CreateAssetMenu(menuName = "Vitals/Temperature", fileName = "Temperature")]
    public class Temperature : VitalData
    {
        public override int GetThreshold(iVital vital) => vital.max / 2;


        public override int GetImpairment(iVital vital) =>
            vital.level > vital.threshold ?
                vital.level - vital.threshold :
            vital.level < -vital.threshold ?
                Mathf.Abs(vital.level + vital.threshold) : 
            0;
    }
}
