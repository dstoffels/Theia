using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Theia.Stats.gear;
using Theia.Items.Base;

namespace Theia.Items
{
    [CreateAssetMenu(menuName = "Item/Inventory Item")]
    public class InventoryItemData : ItemData
    {
        public GearSlotData inventorySlot;
        public GearSlotData alternateSlot;

        public float maxCarryWeight = 0;
        public float maxContainerVolume = 0; 
    }
}