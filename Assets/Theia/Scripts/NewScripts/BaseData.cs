using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;


namespace Stats
{
    public abstract class BaseData : SerializedScriptableObject
    {
        [TextArea(3, 20), PropertyOrder(999)]
        public string description;
    }
}
