using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Stats
{
    /// <summary>
    /// BaseData is a scriptable object that both contains relevant data and acts as an identifier for anything interfaces with it.
    /// </summary>
    public abstract class BaseData : SerializedScriptableObject
    {
        [TextArea(2, 10), PropertyOrder(999)]
        public string description;

        public abstract bool Contains(BaseData stat); // fixme: not all classes use this, configure interface
    }
}
