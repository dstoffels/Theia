using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace Stats
{
    // Network behaviours can't utilize generics, so each stat collection component
    // (attributes, skills, vitals) must manually implement "dictionary" functionality :(
    class Attributes : Mirror.NetworkBehaviour
    {
        // Add 
        public List<AttributeData> data = new List<AttributeData>();
        public Attribute this[string key]
        {
            get { return values[keys.IndexOf(key)]; }
            set { values[keys.IndexOf(key)] = value; }
        }


        List<string> keys = new List<string>();
        public List<string> Keys => keys;
        List<Attribute> values = new List<Attribute>();
        public List<Attribute> Values => values;

        public void Add(string key, Attribute value)
        {
            if (!keys.Contains(key))
            {
                keys.Add(key);
                values.Add(value);
            }
        }

        public void Add(KeyValuePair<string, Attribute> item)
        {
            keys.Add(item.Key);
            values.Add(item.Value);
        }

        public void Clear()
        {
            keys.Clear();
            values.Clear();
        }

        public bool Contains(KeyValuePair<string, Attribute> item) => keys.Contains(item.Key) && values.Contains(item.Value);
        public bool ContainsKey(string key) => keys.Contains(key);

        public void Init()
        {
            if (data.Count != keys.Count)
            {
                Clear();
                foreach (var item in data) Add(item.name, new Attribute(item));
                Debug.Log("Attributes initialized");
            }

        }

        private void OnValidate()
        {
            Init();
        }
    }
}
