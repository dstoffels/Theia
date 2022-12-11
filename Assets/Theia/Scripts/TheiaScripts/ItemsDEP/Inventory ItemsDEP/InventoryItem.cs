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
        public override int volume => base.volume + totalInventoryVolume;
        [ShowInInspector] public int totalInventoryVolume { get; private set; }
        [ShowInInspector] public int maxCapacity => data ? inventorySize.volume : 0;
        [ShowInInspector] public bool isSecured { get; private set; } = true;

        private void UpdateStats()
        {
            SetTotalInventoryVolume();
            SetInventoryWeight();
            SetIsSecured();
        }

        private void SetTotalInventoryVolume() => totalInventoryVolume = utils.Sum<iItem>(inventory, item => item.size.volume);
        private void SetInventoryWeight() => inventoryWeight = utils.Sum<iItem>(inventory, item => item.weight);
        private void SetIsSecured() =>
            isSecured = utils.Reduce<bool, iItem>(inventory, (item, isSecured) =>
                item.size.greatestDimension <= inventorySize.height ? true : isSecured) && 
            totalInventoryVolume <= maxCapacity * capacityThreshold;

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
            totalInventoryVolume + item.size.volume <= maxCapacity &&
            item.size.greatestDimension <= inventorySize.height * 2;

        private void OnValidate()
        {
            UpdateStats();
        }

        [ShowInInspector]
        public static float capacityThreshold = 0.75f;
    }
}


