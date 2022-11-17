using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Stats
{
    public abstract class StatManager<Stat, Data> : SerializedMonoBehaviour where Data: BaseData where Stat : BaseStat<Data>, new()
    {
        public Stat this[string key] => stats[key];
        public Stat this[Data key] => stats[key.name];

        public StatTemplate<Data> template;
        [ShowInInspector, DictionaryDrawerSettings(IsReadOnly =true)]
        protected Dictionary<string, Stat> stats = new Dictionary<string, Stat>();

        public virtual void Init()
        {
            foreach (var data in template.data)
            {
                stats.Add(data.name, new Stat());
                stats[data.name].Init(data);
            }
        }

        private void OnValidate()
        {
            Init();
        }
    }
}
