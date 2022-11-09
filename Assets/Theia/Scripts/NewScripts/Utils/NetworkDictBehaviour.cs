using System.Collections.Generic;
using Sirenix.OdinInspector;
using Stats;
using UnityEngine;

namespace Theia.Utils
{
    public abstract class NetworkDictBehaviour<TValue, TData>
    {
        public List<TData> data = new List<TData>();
        public TValue this[string key]
        {
            get { return values[keys.IndexOf(key)]; }
            set { values[keys.IndexOf(key)] = value; }
        }

        protected abstract void Build(TData data);

        protected List<string> keys = new List<string>();

        public List<string> Keys => keys;

        protected List<TValue> values = new List<TValue>();
        public List<TValue> Values => values;
        public int Count => keys.Count;

        [Button]
        public void Add(string key, TValue value)
        {
            keys.Add(key);
            values.Add(value);
        }

        public void Add(KeyValuePair<string, TValue> item)
        {
            keys.Add(item.Key);
            values.Add(item.Value);
        }

        
    }
}
