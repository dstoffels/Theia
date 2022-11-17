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
        public AttributeData[] secondaryAttributes;
        [AssetList]
        public AttributeData[] recoveryAttributes;

        public bool isFullScale;
        public bool isEquilibrium;
    }
}
