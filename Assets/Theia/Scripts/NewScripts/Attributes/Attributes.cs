using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

namespace Stats
{
    // Network behaviours can't utilize generics, so each stat collection component
    // (attributes, skills, vitals) must manually implement "dictionary" functionality :(
    // Sorry for the mess! 

    public class Attributes : StatManager
    {
        public AttributesTemplate attributesTemplate;
        public Attribute this[string key]
        {
            get { return attributes[keys.IndexOf(key)]; }
            set { attributes[keys.IndexOf(key)] = value; }
        }

        [ShowInInspector]
        List<Attribute> attributes = new List<Attribute>();
        public List<Attribute> Values => attributes;

        public void Add(string key, Attribute value)
        {
            if (!ContainsKey(key))
            {
                keys.Add(key);
                attributes.Add(value);
            }
        }

        public void Add(KeyValuePair<string, Attribute> item)
        {
            if (!ContainsKey(item.Key))
            {
                keys.Add(item.Key);
                attributes.Add(item.Value);
            }
        }

        public void Clear()
        {
            keys.Clear();
            attributes.Clear();
        }

        public bool Contains(KeyValuePair<string, Attribute> item) => keys.Contains(item.Key) && attributes.Contains(item.Value);
        [Button]
        public void Init()
        {
            if (attributesTemplate)
            {
                Clear();
                foreach (var item in attributesTemplate.data)
                {
                    Attribute att = new Attribute(item);
                    Add(item.name, att);
                }
            }

        }

        public void Init(int a)
        {
            Init();
        }

        private void OnValidate()
        {
            if (attributes.Count == 0) Init();
        }
    }


}
