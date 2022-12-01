using System.Collections.Generic;


namespace Theia.IoC
{
    /// <summary>
    /// A dictionary of provider data/values, instantiated in conjunction with iConsumer, if it consumes from multiple providers.
    /// </summary>
    public class Providers<T> : Dictionary<BaseData, T>
    {
        public void Update(BaseData providerData, T providerValue)
        {
            if (!ContainsKey(providerData))
                Add(providerData, providerValue);
            else
                this[providerData] = providerValue;
        }
    }

    public class IntProviders : Providers<int>
    {
        public int GetTotal()
        {
            int total = 0;
            foreach (var value in Values)
                total += value;
            return total;
        }

        public delegate int StatValue(KeyValuePair<BaseData, int> kvp);

        public int Reduce(StatValue callback)
        {
            int total = 0;
            foreach (var provider in this)
                total += callback(provider);
            return total;
        }
    }

}
