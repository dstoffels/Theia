using System.Collections.Generic;


namespace Stats.IoC
{
    /// <summary>
    /// A dictionary of iProviders, keyed by their data property. Used in conjunction with iConsumer, if it consumes from multiple providers.
    /// </summary>
    public class Providers<TProvider, T, TConsumer> : Dictionary<BaseData, T> where TProvider : iProvider<T,TConsumer>
    {
        public void Update(TProvider provider, TConsumer consumer)
        {
            if (!ContainsKey(provider.GetData()))
                Add(provider.GetData(), provider.GetState(consumer));
            else
                this[provider.GetData()] = provider.GetState(consumer);
        }
    }

    public class AttributeProviders : Providers<iAttributeProvider, int, iAttributeConsumer> { }
    public class SkillProviders : Providers<iSkillProvider, int, iSkillConsumer> { } 
}
