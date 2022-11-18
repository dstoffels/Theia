using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Stats
{
    public abstract class StatManager<Stat, Data> : SerializedMonoBehaviour, iStatConsumerManager where Data: BaseData where Stat : BaseStat<Data>, new()
    {
        public Stat this[string key] => stats[key];
        public Stat this[Data key] => stats[key.name];

        public StatTemplate<Data> template;
        [ShowInInspector, DictionaryDrawerSettings(IsReadOnly =true)]
        protected Dictionary<string, Stat> stats = new Dictionary<string, Stat>();

        public Dictionary<string, Stat>.ValueCollection all => stats.Values;

        public void InitializeTemplate()
        {
            if (template)
            {
                foreach (var data in template.data)
                {
                    stats.Add(data.name, new Stat());
                    stats[data.name].Init(data);
                }
            }
        }

        public void NotifyConsumers() { foreach (var stat in all) stat.Update(); }

        public void Init(iStatProvider<Data> provider)
        {
            throw new NotImplementedException();
        }
    }
}
