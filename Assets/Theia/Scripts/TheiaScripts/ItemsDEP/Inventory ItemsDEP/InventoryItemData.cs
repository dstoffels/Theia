using System.Collections.Generic;
using UnityEngine;
using Theia.Items.deprecated;
using Sirenix.OdinInspector;

namespace Theia.Items.refactor
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