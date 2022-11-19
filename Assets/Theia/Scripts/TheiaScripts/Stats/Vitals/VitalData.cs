using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Stats.Values;

namespace Stats
{
    [CreateAssetMenu(menuName ="Vitals/New Vital")]
    public class VitalData : BaseData
    {
        public AttributeData primaryAttribute;
        [AssetList]
        public List<AttributeData> secondaryAttributes;
        [AssetList]
        public List<AttributeData> recoveryAttributes;

        public bool isFullScale;
        public bool isEquilibrium;

        public override bool Contains(BaseData stat)
        {
            return stat == primaryAttribute || secondaryAttributes.Contains((AttributeData)stat) || recoveryAttributes.Contains((AttributeData)stat);
        }
    }
}
