using Sirenix.OdinInspector;
using Theia.IoC;
using Theia.Items;
using Theia.Items.Base;
using UnityEngine;

namespace Theia.Stats.gear
{
    [HideReferenceObjectPicker]
    public class GearSlot : DataClient<GearSlotData>, iWearableItemSlot<InventoryItem>, iWeightProvider
    {
        [ListDrawerSettings(Expanded = true), ReadOnly]
        public InventoryItem[] slots;

        public ItemSize inventorySize => throw new System.NotImplementedException();

        public int weight => throw new System.NotImplementedException();

        public ItemSize size => throw new System.NotImplementedException();

        public InventoryItem Equip(InventoryItem newItem)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] == null)
                {
                    slots[i] = newItem;
                    return default;
                }                    
            }
            return newItem;
        }


        public InventoryItem Remove(InventoryItem item)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] == item)
                {
                    slots[i] = null;
                    return item;
                }
            }
            return null;
        }

        public iItem StowItemInContainer(iItem newItem, InventoryItem container)
        {
            foreach (var wornContainer in slots)
                if (wornContainer == container)
                    return container.StowItem(newItem);
            Debug.Log($"FAILED: {container.name} not found in player gear. Container must be equipped first.");
            return newItem;
        }

        public iItem RemoveItemFromContainer(iItem item, InventoryItem container)
        {
            foreach (var wornContainer in slots)
                if (wornContainer == container)
                    return container.RemoveItem(item);
            Debug.Log($"FAILED: {container.name} not found in player gear or {item.name} not found in inventory.");
            return default;
        }

        // PROVIDER INTERFACE
        public int GetWeight() => utils.Sum<InventoryItem>(slots, item => item ? item.weight : 0);

        public override void Init(GearSlotData data)
        {
            base.Init(data);
            slots = new InventoryItem[data.numSlots];
        }
    }
}
