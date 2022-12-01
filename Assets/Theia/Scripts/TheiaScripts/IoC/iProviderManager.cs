using Theia.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Theia.IoC
{
    public interface iProviderManager<TProvider>
    {
        TProvider[] GetProviders();
    }
}
