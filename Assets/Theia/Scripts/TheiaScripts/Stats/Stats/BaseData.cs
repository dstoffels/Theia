using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Stats
{
    public abstract class BaseData : SerializedScriptableObject
    {
        [TextArea(2, 10), PropertyOrder(999)]
        public string description;
    }
}
