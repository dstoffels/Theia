using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Stats
{
    // Vitals each calculate their stats differently so VitalData is an abstract class to build off,
    // which provides the option for each Scriptable Vital to implement their own methods.
    public class VitalData : BaseData
    {
        public AttributeData primaryAttribute;
        [AssetList]
        public List<AttributeData> secondaryAttributes;

        public override bool Contains(BaseData stat)
        {
            return stat == primaryAttribute || secondaryAttributes.Contains((AttributeData)stat);
        }

        public virtual int GetMax(List<iStatProvider<AttributeData>> providers)
        {
            int max = 0;
            foreach (var provider in providers)
            {
                var stat = provider.GetStatValue();
                max += stat.data == primaryAttribute ? stat.value * 2 : secondaryAttributes.Contains(stat.data) ? stat.value : 0;
            }
            return max;
        }
        public virtual int GetMin(Vital vital) => -vital.max;
        public virtual int GetThreshold(Vital vital) => vital.max / 2;
        public virtual float GetDebility(Vital vital) => Mathf.Min(0, vital.level);
        public virtual float GetRecoveryPointsPerPulse(List<iStatProvider<AttributeData>> providers)
        {
            foreach (var provider in providers)
            {
                var stat = provider.GetStatValue();
                if (stat.data == primaryAttribute)
                    return stat.value / 25; // TODO: sort out math
            }
            return 0;
        }
    }
}
