using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Stats
{
    public abstract class ConsumerStat<TData, TConsumedData> : BaseStat<TData>, iStatConsumer<TConsumedData>
        where TData : BaseData
        where TConsumedData : BaseData
    {
        protected ProviderValues<TConsumedData> providerValues = new ProviderValues<TConsumedData>();
        public virtual void Subscribe(iStatProvider<TConsumedData> provider)
        {
            var stat = provider.GetStatValue();
            if (data.Contains(stat.data))
            {
                provider.AddConsumer(this);
                Update(stat);
            }
        }

        public virtual void Update(StatValue<TConsumedData> providerValue) => providerValues.Add(providerValue);
    }
}
