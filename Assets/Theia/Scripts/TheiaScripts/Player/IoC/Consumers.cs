using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Stats;
using UnityEngine;

namespace Theia.IoC
{
    /// <summary>
    /// A set of iConsumers, used in conjunction with iProvider.
    /// </summary>
    public class Consumers<TConsumer, TProvider> : List<TConsumer> where TConsumer : iConsumer<TProvider>
    {
        new public void Add(TConsumer consumer)
        {
            if(!Contains(consumer))
                base.Add(consumer);
        }
        public void Notify(TProvider provider)
        {
            foreach (var consumer in this)
                consumer.Notify(provider);
        }
    }

    public class AttributeConsumers : Consumers<iAttributeConsumer, iAttributeProvider> { }
    public class SkillConsumers : Consumers<iSkillConsumer, iSkillProvider> { }
    public class BodyPartConsumers : Consumers<iBodyPartConsumer, iBodyPartProvider> { }    
}
