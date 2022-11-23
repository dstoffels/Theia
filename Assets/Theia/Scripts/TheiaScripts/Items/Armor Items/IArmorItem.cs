using System.Collections.Generic;
using InventoryStuff.Armor;
using StatsOLD;

namespace Items.Armor
{
    public interface IArmorItem
    {
        ArmorSlotData armorSlot { get; }
        List<OrganData> coverage { get; }
        float hindrance { get; }
        float protection { get; }
        bool isImmuneToSoftSpots { get; }
    }
}