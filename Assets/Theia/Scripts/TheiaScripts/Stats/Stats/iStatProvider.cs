using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Stats
{
    /// <summary>
    /// Must hold a list of its observers to notify whenever it changes
    /// </summary>
    public interface iStatProvider<TProviderData> where TProviderData : BaseData
    {
        StatValue<TProviderData> GetStatValue();
        void AddConsumer(iStatConsumer<TProviderData> observer);
        void NotifyObservers();
    }

    public interface iStatConsumer<TProviderData> where TProviderData : BaseData
    {
        void Update(StatValue<TProviderData> providerValue);
        void Subscribe(iStatProvider<TProviderData> provider);
    }

    public interface iStatProviderManager<TProviderData> where TProviderData : BaseData
    {
        iStatProvider<TProviderData>[] Get();
    }


    public interface iStatConsumerManager<TConsumerData> where TConsumerData : BaseData
    {
        void SubscribeAll(iStatProviderManager<TConsumerData> providers);
    }
    public struct StatValue<TData> where TData : BaseData
    {
        public TData data;
        public int value;

        public StatValue(TData data, int value)
        {
            this.data = data;
            this.value = value;
        }
    }

    public class ProviderValues<TProviderData> :  Dictionary<string, StatValue<TProviderData>> where TProviderData : BaseData 
    {
        public int this[TProviderData data] => this[data.name].value;
        public void Add(StatValue<TProviderData> statValue)
        {
            if (!ContainsKey(statValue.data.name)) Add(statValue.data.name, statValue);
            else this[statValue.data.name] = statValue;
        }

        public delegate int SumCB(StatValue<TProviderData> statValue);

        public int Reduce(SumCB sumCB)
        {
            int total = 0;
            foreach (var statValue in Values) 
                total += sumCB(statValue);
            return total;
        }
    }
}