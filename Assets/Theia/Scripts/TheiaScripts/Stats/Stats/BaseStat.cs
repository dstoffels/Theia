using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Stats
{
    public abstract class BaseStat<TData> where TData : BaseData
    {
        /// <summary>
        /// StatData Scriptable object that is "plugged in" to the stat to provide le data.
        /// </summary>
        [HideInInspector]
        public TData data;
        public string name => data.name;
        public string description => data.description;
        public virtual void Init(TData data) => this.data = data;
    }
}
