using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Stats
{
    /// <summary>
    /// Must hold a list of its observers to notify whenever it changes
    /// </summary>
    public interface iStatProvider<TData> where TData : BaseData
    {
        StatValue<TData> GetStatValue();
        void AddConsumer(iStatConsumer<TData> observer);
        void NotifyObservers();
    }

    public interface iStatConsumer<TData> where TData : BaseData
    {
        void Update(iStatProvider<TData> provider);
        void Subscribe(iStatProvider<TData> provider);
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

    public interface iStatProviderManager<TProviderData> where TProviderData : BaseData
    {
        iStatProvider<TProviderData>[] Get();
    }


    public interface iStatConsumerManager<TConsumerData> where TConsumerData : BaseData
    {
        void SubscribeAll(iStatProviderManager<TConsumerData> providers);
    }

}