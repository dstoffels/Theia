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
        public TData _data;
        public TData data => _data;
        public string name => _data.name;
        public string description => _data.description;
        public virtual void Init(TData data) => this._data = data;
    }
}
