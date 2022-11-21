using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Stats
{
    [CreateAssetMenu(menuName = "Vitals/Stamina")]
    public class Stamina : VitalData
    {
        public override int GetMin(Vital vital)
        {
            throw new NotImplementedException();
        }

        public override int GetThreshold(Vital vital)
        {
            throw new NotImplementedException();
        }
        public override float GetDebility(Vital vital)
        {
            throw new NotImplementedException();
        }
        public override float GetRecoveryPointsPerPulse(List<iStatProvider<AttributeData>> providers)
        {
            throw new NotImplementedException();
        }
    }
}
