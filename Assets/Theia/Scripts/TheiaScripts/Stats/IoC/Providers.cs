using System.Collections.Generic;


namespace Stats.IoC
{
    /// <summary>
    /// A dictionary of iProviders, keyed by their data property. Used in conjunction with iConsumer, if it consumes from multiple providers.
    /// </summary>
    public class Providers<TProvider, TConsumer> : Dictionary<BaseData, TProvider> where TProvider : iProvider<TConsumer>
    {
        public void Update(TProvider provider, TConsumer consumer)
        {
            if (!ContainsKey(provider.GetData()))
                Add(provider.GetData(), provider);
            else
                this[provider.GetData()] = provider;
        }
    }

    public abstract class LevelProviders<TProvider, TConsumer> : Providers<TProvider, TConsumer> where TProvider : iLevelProvider<TConsumer>
    {
        public delegate int ProviderReducer(TProvider provider);

        public int Reduce(ProviderReducer callback, int initialValue = 0)
        {
            int total = initialValue;
            foreach (var provider in Values)
                total += callback(provider);
            return total;
        }
    }

    public class AttributeProviders : LevelProviders<iAttributeProvider, iAttributeConsumer> { }
    public class SkillProviders : LevelProviders<iSkillProvider, iSkillConsumer> { }    
}
