using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Stats
{
    public abstract class BaseStat<Data> where Data : BaseData
    {
        /// <summary>
        /// Scriptable object with all stat data that is "plugged in" to the stat.
        /// </summary>
        [HideInInspector]
        public Data data;
        public string name => data.name;
        public string description => data.description;
        public void Init(Data data) => this.data = data;

    }

    public interface iStat
    {
        string name { get; }
        int level { get; }
    }
}
