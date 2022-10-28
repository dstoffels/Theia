using InventoryStuff.Armor;
using Stats;
using System.Collections.Generic;

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