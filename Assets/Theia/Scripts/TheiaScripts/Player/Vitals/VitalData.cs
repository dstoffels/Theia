using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Theia.IoC;
using Theia.Stats.attributes;

namespace Theia.Stats.vitals
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

        public override bool Contains(BaseData stat) =>
             stat == primaryAttribute || secondaryAttributes.Contains((AttributeData)stat);


        public virtual int GetMax(IntProviders providers) => providers.Reduce(
            att =>
                att.Key == primaryAttribute ?
                    att.Value * 2 :
                secondaryAttributes.Contains((AttributeData)att.Key) ?
                    att.Value :
                0);
        
        public virtual int GetMin(iVital vital) => -vital.max;
        public virtual int GetThreshold(iVital vital) => 0;
        public virtual int GetImpairment(iVital vital) => Mathf.Min(0, vital.level);

        [ShowInInspector, ReadOnly]
        public static int FULL_RECOVERY_TIME_IN_MIN = 2;
        /// <summary>
        /// The amount of time(ms) to recover 1pt, based on full recovery from min-max over X time.
        /// </summary>
        /// <param name="vital"></param>
        /// <returns></returns>
        public int GetRecoveryRate(iVital vital) => FULL_RECOVERY_TIME_IN_MIN * 60 * 1000 / (vital.max * 2);



    }
}
