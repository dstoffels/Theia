using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Stats;
using UnityEngine;

namespace Stats.IoC
{
    /// <summary>
    /// A list of iConsumers, used in conjunction with iProvider.
    /// </summary>
    public class Consumers<T> : List<iConsumer<T>>
    {
        public void Notify(iProvider<T> provider)
        {
            foreach (var consumer in this)
                consumer.Update(provider);
        }
    }
}
