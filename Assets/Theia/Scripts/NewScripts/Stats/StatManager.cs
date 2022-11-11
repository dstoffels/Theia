using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Stats
{
    /// <summary>
    /// A dictionary wrapper component for easy access to stats (attributes, skills, vitals)
    /// </summary>
    /// <typeparam name="TStat"></typeparam>
    /// <typeparam name="TData"></typeparam>
    public abstract class StatManager<TStat, TData> : SerializedMonoBehaviour where TStat : Stat<TData> where TData : BaseData
    {
        public StatsTemplate<TData> template;

        public TStat this[string key] => stats[key];
        [ShowInInspector, DictionaryDrawerSettings(ValueLabel = "", KeyLabel = "", IsReadOnly = true)]
        protected Dictionary<string, TStat> stats = new Dictionary<string, TStat>();

        public Dictionary<string, TStat>.KeyCollection Keys => stats.Keys;
        public Dictionary<string, TStat>.ValueCollection Values => stats.Values;

        public void Add(string key, TStat value)
        {
            if (!ContainsKey(key)) stats.Add(key, value);
        }

        public void Clear() => stats.Clear();

        public bool ContainsKey(string key) => stats.ContainsKey(key);

        public virtual void Init()
        {
            Clear();
            if (template)
                foreach (var data in template.data)
                    GenerateStatFromData(data);
        }

        protected abstract void GenerateStatFromData(TData data);
    }
}
