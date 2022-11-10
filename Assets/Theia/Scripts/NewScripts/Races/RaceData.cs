using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;


namespace Stats
{
    [CreateAssetMenu(menuName = "Race")]
    public class RaceData : BaseData 
    {
        public Dictionary<AttributeData, int> attributeModifiers = new Dictionary<AttributeData, int>();
    }
}
