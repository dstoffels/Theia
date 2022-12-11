using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Theia.Stats.gear;

namespace Theia.Items.Base
{
    public class InventoryItem : MaterialItem<InventoryItemData>, iContainer
    {
        public GearSlotData slot => data.inventorySlot;
        public ItemSize inventorySize => data.inventorySize;
        [ShowInInspector, ListDrawerSettings(IsReadOnly = true, Expanded = true)] 
        public List<iItem> inventory = new List<iItem>();

        public override int weight => base.weight + inventoryWeight;
        private int inventoryWeight { get; set; }
        public override int volume => base.volume + currentInventoryVolume;
        [ShowInInspector] public int currentInventoryVolume { get; private set; }
        [ShowInInspector] public int maxCapacity => data ? inventorySize.volume : 0;
        [ShowInInspector] public bool isSecured { get; private set; } = true;

        private void UpdateStats()
        {
            currentInventoryVolume = 0;
            inventoryWeight= 0;
            bool hasOversizedItem = false;
            foreach (var item in inventory)
            {
                currentInventoryVolume += item.size.volume;
                inventoryWeight += item.weight;
                hasOversizedItem = item.size.greatestDimension > inventorySize.height * 2 ? true : hasOversizedItem;
            }
            isSecured = !hasOversizedItem;
        }

        [Button]
        public iItem RemoveItem(iItem item)
        {
            inventory.Remove(item);
            UpdateStats();
            return item;
        }

        [Button]
        public iItem StowItem(iItem newItem)
        {
            if (CanStowItem(newItem) && (Object)newItem != this)
            {
                if (!inventory.Contains(newItem))
                {
                    inventory.Add(newItem);
                    UpdateStats();
                }
                return default;
            }
            else
            {
                Debug.Log($"Cannot put {newItem.name} in {name}");
                return newItem;
            }
        }

        private bool CanStowItem(iItem item) =>
            currentInventoryVolume + item.size.volume <= maxCapacity &&
            item.size.greatestDimension <= inventorySize.height * 2;

        private void OnValidate()
        {
            UpdateStats();
        }
    }
}


