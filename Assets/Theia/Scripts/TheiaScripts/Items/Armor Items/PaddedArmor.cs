using System.Collections.Generic;
using InventoryStuff.Armor;
using Mats;
using StatsOLD;

namespace Items.Armor
{
    public class PaddedArmor : Item<FabricMat, ArmorItemData>, IArmorItem
    {
        public ArmorSlotData armorSlot => data.armorSlot;

        public List<OrganData> coverage => data.coverage;

        public float hindrance => data.hindrance;

        public float protection => data.protectionFactor;

        public bool isImmuneToSoftSpots => false;
    }
}
