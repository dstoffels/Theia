using Stats.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stats.IoC
{
    /// <summary>
    /// A dictionary of iProviders, keyed by their data property. Used in conjunction with iConsumer, if it consumes from multiple providers.
    /// </summary>
    public class Providers<T> : Dictionary<BaseData, T>
    {
        public void Update(iProvider<T> provider, iConsumer<T> consumer)
        {
            if (!ContainsKey(provider.GetData()))
                Add(provider.GetData(), provider.GetValue(consumer));
            else
                this[provider.GetData()] = provider.GetValue(consumer);
        }
    }
}
