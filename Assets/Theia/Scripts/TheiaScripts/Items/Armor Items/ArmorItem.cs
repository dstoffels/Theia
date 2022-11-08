using InventoryStuff.Armor;
using Sirenix.OdinInspector;
using StatsOLD;
using System.Collections.Generic;

namespace Items.Armor
{
    public abstract class ArmorItem<Material> : Item<Material, ArmorItemData>, IArmorItem
        where Material : Mats.Mat
    {
        public ArmorSlotData armorSlot => data.armorSlot;

        public List<OrganData> coverage => data.coverage;

        // fixme: these fields must be properties which draw from the referenced ArmorItemData & Mat
        public float protection => data.protectionFactor;

        public abstract float hindrance { get; }

        public abstract bool isImmuneToSoftSpots { get; } //fixme: override in Rigid armor when adding accessories
    }
}
