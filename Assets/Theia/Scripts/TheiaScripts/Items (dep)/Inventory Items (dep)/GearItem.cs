using System.Collections.Generic;
using InventoryStuff;
using Mats;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Items
{
    public class GearItem : ItemBase<FabricMat, InventoryItemData>, IWearableItem, IContainer
    {
        public List<iItem> storedItems = new List<iItem>();
        public bool isSecured { get; set; }

        public GearSlotData slot => data.inventorySlot;
        public float maxCarryWeight => data.maxCarryWeight;
        public float currentCarryWeight
        {
            get
            {
                float total = 0;
                foreach (var item in storedItems)
                    total += item.weight;
                return total;
            }
        }

        [ShowInInspector] public float maxContainerVolume => data ? data.maxContainerVolume : 0;
        [ShowInInspector] public float currentVolume
        {
            get
            {
                float total = 0;
                foreach (var item in storedItems)
                    total += item.volume;
                return total;
            }
        }

        [Button]
        public void Wear(Player player)
        {
            //var targetSlot = player.gear.slots[slot];
            //var curentlyWornItem = targetSlot.WearItem(this);

            //transform.SetParent(player.transform); // TODO: will eventually have to attach this to player bones & wearableslots

            //if (curentlyWornItem != null)
            //    player.gear.GrabItem(curentlyWornItem);
        }

        [Button]
        public void Remove()
        {
            //owner.gear.GrabItem(currentSlot.RemoveItem(this));
        }


        public iItem TakeItem(iItem item)
        {
            storedItems.Remove(item);
            return item;
        }

        public bool StowItem(iItem newItem)
        {
            if (DoesItemFit(newItem))
            {
                storedItems.Add(newItem);
                return true;
            }
            else
            {
                //owner.gear.GrabItem(newItem);
                Debug.Log("Dis bish don' fit in " + name);
                return false;
            }
        }

        private bool DoesItemFit(iItem item)
        {
            var maxLength = isSecured ? height : height * 2;                        // if container must be secured, item length cannot exceed container length. If not, up to 2x container length is allowed.

            if (item.width > width) return false;                                   // is item wider than container?
            if (item.height > maxLength) return false;                              // is item too tall for container?
            if (currentVolume + item.volume > maxContainerVolume) return false;     // is the container too full to handle the additional volume of the item?
            return true;
        }
    }
}


