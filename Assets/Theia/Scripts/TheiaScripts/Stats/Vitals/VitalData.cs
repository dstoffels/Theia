using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Stats.Values;

namespace Stats
{
    /// <summary>
    /// Vitals each calculate their stats differently so VitalData contains default members to work from,
    /// providing flexibility for each to override as needed.
    /// </summary>
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
                att.data == primaryAttribute ? 
                    att.value * 2 : 
                secondaryAttributes.Contains(att.data) ? 
                    att.value : 
                0
            );  
        
        public virtual int GetMin(Vital vital) => -vital.max;
        public virtual int GetThreshold(Vital vital) => 0;
        public virtual int GetImpairment(Vital vital) => Mathf.Min(0, vital.level);

        [ShowInInspector, ReadOnly]
        public static int FULL_RECOVERY_TIME_IN_MIN = 2;
        /// <summary>
        /// The amount of time(ms) to recover 1pt, based on full recovery from min-max over X time.
        /// </summary>
        /// <param name="vital"></param>
        /// <returns></returns>
        public int GetRecoveryRate(Vital vital) => FULL_RECOVERY_TIME_IN_MIN * 60 * 1000 / (vital.max * 2);



    }
}
