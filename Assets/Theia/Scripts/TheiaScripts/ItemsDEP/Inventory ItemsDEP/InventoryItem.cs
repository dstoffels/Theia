using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Theia.Stats.gear;
using static UnityEditor.Progress;

namespace Theia.Items.Base
{
    public class InventoryItem : MaterialItem<InventoryItemData>, iContainer
    {
        public GearSlotData slot => data.inventorySlot;
        public ItemSize inventorySize => data.inventorySize;
        [ShowInInspector, ListDrawerSettings(IsReadOnly = true, Expanded = true)] 
        public List<iItem> inventory = new List<iItem>();
        //public bool isSecured { get; set; }

        public override int weight => base.weight + inventoryWeight;
        private int inventoryWeight { get; set; }
        public override int volume => base.volume + currentInventoryVolume;
        [ShowInInspector] public int currentInventoryVolume { get; private set; }
        [ShowInInspector] public int maxInventoryVolume => data ? inventorySize.volume : 0;

        private void SetCurrentVolumeAndWeight()
        {
            currentInventoryVolume = 0;
            inventoryWeight= 0;
            foreach (var item in inventory)
            {
                currentInventoryVolume += item.size.volume;
                inventoryWeight += item.weight;
            }
                
        }

        [Button]
        public iItem RemoveItem(iItem item)
        {
            inventory.Remove(item);
            SetCurrentVolumeAndWeight();
            return item;
        }

        [Button]
        public iItem StowItem(iItem newItem)
        {
            if (CanStowItem(newItem))
            {
                if (!inventory.Contains(newItem))
                {
                    inventory.Add(newItem);
                    SetCurrentVolumeAndWeight();
                }
                return default;
            }
            else
            {
                Debug.Log("Dis bish don' fit in " + name);
                return newItem;
            }
        }

        private bool CanStowItem(iItem item) =>
            currentInventoryVolume + item.size.volume <= maxInventoryVolume &&
            item.size.height <= inventorySize.height * 2 &&
            item.size.width <= inventorySize.height * 2 &&
            item.size.depth <= inventorySize.height * 2;
    }
}


