using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Stats
{
    public abstract class StatManager<TStat, TData> : SerializedMonoBehaviour
        where TData: BaseData 
        where TStat : BaseStat<TData>, new()
    {
        protected bool initialized;
        public TStat this[string key] => stats[key];
        public TStat this[TData key] => stats[key.name];

        public StatTemplate<TData> template;
        [ShowInInspector, DictionaryDrawerSettings(IsReadOnly =true)]
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

        public void InitializeTemplate()
        {
            if (template)
            {
                foreach (var data in template.data)
                {
                    stats.Add(data.name, new TStat());
                    this[data].Init(data);
                }
            }
        }
    }
}
