using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Theia.Stats.gear;

namespace Theia.Items.Base
{
    public class InventoryItem : MaterialItem<InventoryItemData>, iContainer
    {
        public GearSlotData slot => data.inventorySlot;
        [ShowInInspector] public List<iItem> inventory = new List<iItem>();
        //public bool isSecured { get; set; }

        public override int weight => base.weight + inventoryWeight;

        private int inventoryWeight { get; set; }

        [ShowInInspector] public int maxInventoryVolume => data ? data.maxInventoryVolume : 0;

        [ShowInInspector] public int currentInventoryVolume { get; private set; }

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
            if (CanFit(newItem))
            {
                inventory.Add(newItem);
                SetCurrentVolumeAndWeight();
                return default;
            }
            else
            {
                Debug.Log("Dis bish don' fit in " + name);
                return newItem;
            }
        }

        private bool CanFit(iItem item) =>
            currentInventoryVolume + item.size.volume <= maxInventoryVolume && item.size.CanStow(this);
    }
}


