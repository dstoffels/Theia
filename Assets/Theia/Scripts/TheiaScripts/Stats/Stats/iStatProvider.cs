using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Stats
{
    public interface iStatProvider
    {
        int GetLevel(BaseData stat);
        void NotifyConsumers(BaseData statData);
    }

    public interface iStatConsumer<TProvider> where TProvider : iStatProvider
    {
        void SetProvider(TProvider provider);

        void Update();
    }

}