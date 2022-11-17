using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Stats
{
    public interface iProvider
    {
        void NotifyDependents(BaseData statData);
    }

    public interface iConsumer<TProvider> where TProvider : iProvider
    {
        void SetProvider(TProvider provider);
    }

}