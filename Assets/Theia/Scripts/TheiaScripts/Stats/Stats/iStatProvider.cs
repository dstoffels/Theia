using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Stats
{
    public interface iStatProvider<TProviderData> where TProviderData : BaseData
    {
        int GetLevel(TProviderData stat);
    }

    public interface iStatConsumer
    {
        void Update();
    }

    public interface iObservable
    {
        void AddProvider(iStatConsumerManager provider);
    }

    public interface iStatConsumerManager
    {
        void NotifyConsumers();
    }

}