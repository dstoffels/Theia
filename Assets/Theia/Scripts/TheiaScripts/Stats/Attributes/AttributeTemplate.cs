using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace StatsOLD
{
    public class AttributeTemplate : SerializedScriptableObject
    {
        [AssetList]
        public List<AttributeData> attributeList = new List<AttributeData>();
    }
}
