using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Stats
{
    public abstract class StatManager<TStat, TData> : SerializedMonoBehaviour
        where TStat : LinkableObject<TData>, new()
        where TData: BaseData 
    {
        public TStat this[string key] => stats[key];
        public TStat this[TData key] => stats[key.name];

        public StatTemplate<TData> template;
        [ShowInInspector, DictionaryDrawerSettings(IsReadOnly =true, KeyLabel = "", ValueLabel = ""), HideLabel]
        protected Dictionary<string, TStat> stats = new Dictionary<string, TStat>();

        public TStat[] all
        {
            get
            {
                TStat[] _all = new TStat[stats.Count];
                stats.Values.CopyTo(_all,0);
                return _all;
            }
        }

        public virtual void InitializeTemplate()
        {
            if (template)
            {
                foreach (var data in template.data)
                {
                    if (!stats.ContainsKey(data.name))
                    {
                        stats.Add(data.name, new TStat());
                        this[data].Init(data);
                    }
                }
            }
        }
    }
}
