using InventoryStuff.Armor;
using Stats;
using System.Collections.Generic;
using UnityEngine;

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
