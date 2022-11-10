using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Stats
{
    /// <summary>
    /// A dictionary-like component that stores stats.
    /// </summary>
    /// <typeparam name="TStat"></typeparam>
    /// <typeparam name="TData"></typeparam>
    public abstract class StatManager<TStat, TData> : SerializedMonoBehaviour where TStat : Stat<TData> where TData : BaseData
    {
        public StatsTemplate<TData> template;

        public TStat this[string key]
        {
            get { return values[keys.IndexOf(key)]; }
            set { values[keys.IndexOf(key)] = value; }
        }

        protected List<string> keys = new List<string>();
        public List<string> Keys => keys;

        [ShowInInspector]
        protected List<TStat> values = new List<TStat>();
        public List<TStat> Values => values;

        public void Add(string key, TStat value)
        {
            if (!ContainsKey(key))
            {
                keys.Add(key);
                values.Add(value);
            }
        }

        public void Add(KeyValuePair<string, TStat> item)
        {
            if (!ContainsKey(item.Key))
            {
                keys.Add(item.Key);
                values.Add(item.Value);
            }
        }

        public void Clear()
        {
            keys.Clear();
            values.Clear();
        }

        public bool ContainsKey(string key) => keys.Contains(key);
        public bool Contains(KeyValuePair<string, TStat> item) => keys.Contains(item.Key) && values.Contains(item.Value);

        [Button]
        public virtual void Init()
        {
            if (template)
            {
                Clear();
                foreach (var data in template.data)
                {
                    GenerateStatFromData(data);
                }
            }
        }

        protected abstract void GenerateStatFromData(TData data);

        private void OnValidate()
        {
            if (values.Count == 0) Init();
        }
    }
}
