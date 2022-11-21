using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Stats
{
    // Vitals each calculate their stats differently so VitalData is an abstract class to build off,
    // which provides the option for each Scriptable Vital to implement their own methods.
    [CreateAssetMenu(menuName = "Vitals/Default Vital", fileName = "Stamina")]
    public class VitalData : BaseData
    {
        public AttributeData primaryAttribute;
        [AssetList]
        public List<AttributeData> secondaryAttributes;

        public override bool Contains(BaseData stat)
        {
            return stat == primaryAttribute || secondaryAttributes.Contains((AttributeData)stat);
        }

        public virtual int GetMax(ProviderValues<AttributeData> providerValues) => 
            providerValues.Reduce(att => 
                att.data == primaryAttribute ? att.value * 2 : 
                secondaryAttributes.Contains(att.data) ? att.value : 0
            );  
        
        public virtual int GetMin(Vital vital) => -vital.max;
        public virtual int GetThreshold(Vital vital) => vital.max / 2;
        public virtual float GetDebility(Vital vital) => Mathf.Min(0, vital.level);
        public virtual float GetRecoveryPointsPerPulse(ProviderValues<AttributeData> providerValues) =>        
            providerValues[primaryAttribute] / 25; // TODO: sort out math
    }
}
