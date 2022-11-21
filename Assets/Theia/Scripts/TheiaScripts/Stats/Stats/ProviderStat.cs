using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Stats.Values;

namespace Stats
{
    public abstract class ProviderStat<TData, TConsumedData> : ConsumerStat<TData, TConsumedData>, iStatProvider<TData>
        where TData : BaseData
        where TConsumedData : BaseData
    {
        private List<iStatConsumer<TData>> observers = new List<iStatConsumer<TData>>();
        public void AddConsumer(iStatConsumer<TData> observer) { if (!observers.Contains(observer)) observers.Add(observer); }

        public void NotifyObservers() { foreach (var observer in observers) observer.Update(GetStatValue()); }

        public abstract StatValue<TData> GetStatValue();
    }
}
