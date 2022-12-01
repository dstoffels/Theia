using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Stats
{
    /// <summary>
    /// BaseData is a scriptable object base for creating data assets that are consumed by DataClient objects. 
    /// Primarily for storing raw data, "Data" objects can also store logic, while their clients are responsible 
    /// for managing state.
    /// </summary>
    public abstract class BaseData : SerializedScriptableObject
    {
        [TextArea(2, 10), PropertyOrder(999)]
        public string description;

        public virtual bool Contains(BaseData stat) => stat.Contains(this);
    }
}
