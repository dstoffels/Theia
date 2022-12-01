using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Theia
{
    /// <summary>
    /// A base class which derives its data from a ScriptableObject (BaseData).
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    public abstract class DataClient<TData> 
        where TData : BaseData
    {
        /// <summary>
        /// StatData Scriptable object that is "plugged in" to the stat to provide le data.
        /// </summary>
        [HideInInspector]
        public TData data;
        public virtual string name => data.name;
        public string description => data.description;
        public virtual void Init(TData data) => this.data = data;
    }
}
