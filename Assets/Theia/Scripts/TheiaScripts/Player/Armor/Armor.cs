using Sirenix.OdinInspector;
using Stats;
using System.Collections.Generic;
using Theia.IoC;
using UnityEngine;

namespace Theia.Stats.armor
{
    public class Armor : DataClientManager<ArmorSlotData, ArmorSlot>, iProviderManager<iArmorProvider>
    {
        public iArmorProvider[] GetProviders() => all;
    }
}
