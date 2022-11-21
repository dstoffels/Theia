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
        void Update(iStatProvider<TProviderData> provider);
        void Subscribe(iStatProvider<TProviderData> provider);
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