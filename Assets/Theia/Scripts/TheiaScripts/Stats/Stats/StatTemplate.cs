using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Stats
{
    public class StatTemplate<StatData> : SerializedScriptableObject where StatData : BaseData
    {
        [AssetList]
        public StatData[] data;
    }
}
