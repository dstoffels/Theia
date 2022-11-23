using System.Collections.Generic;
using InventoryStuff.Armor;
using UnityEngine;
using StatsOLD;

namespace Items.Armor
{
    [CreateAssetMenu(menuName = "Armor/Armor")]
    public class ArmorItemData : ItemData
    {
        public ArmorSlotData armorSlot;
        public List<OrganData> coverage;
        public bool consumesArmsSlot;

        public float protectionFactor;
        public float hindrance;
    }
}
