using Sirenix.OdinInspector;
using UnityEngine;
using Theia.Items.Base;

namespace Theia.Stats.gear
{
    public class Gear : DataClientManager<GearSlotData, GearSlot>, iEquippable<InventoryItem>
    {
        [ShowInInspector, ReadOnly]
        public int weight { get; private set; }
        public void SetWeight() => weight = utils.Sum<GearSlot>(all, slot => slot.GetWeight());

        [Button]
        public InventoryItem Equip(InventoryItem container)
        {
            container = this[container.slot].Equip(container);
            SetWeight();
            return container;
        }

        [Button]
        public InventoryItem Remove(InventoryItem container)
        {
            container = this[container.slot].Remove(container);
            SetWeight();
            return container;
        }

        [ShowInInspector, ReadOnly]
        public InventoryItem defaultContainer { get; private set; }

        [Button]
        public void SetDefaultContainer(InventoryItem container) => defaultContainer = container;

        [Button]
        public iItem StowItemInContainer(iItem newItem, InventoryItem container= null)
        {
            container = container ? container : defaultContainer;
            newItem = this[container.slot].StowItemInContainer(newItem, container);
            SetWeight();
            return newItem;
        }

        [Button]
        public iItem RemoveItemFromContainer(iItem item, InventoryItem container)
        {
            item = this[container.slot].RemoveItemFromContainer(item, container);
            SetWeight();
            return item;
        }


        // TODO: will this method return/display a message for feedback?
        // TODO: may need a variation of this method to swap and/or automatically get and store an item in next available inventory
        //public void GrabItem(iItem item)
        //{
        //    // Try to get the item with right hand first
        //    if (rightHand.item == null)
        //    {
        //        rightHand.item = item;
        //        item.currentSlot = rightHand;
        //    }

        //    else if (leftHand.item == null)
        //    {
        //        leftHand.item = item;
        //        item.currentSlot = leftHand;
        //    }
        //    else
        //    {
        //        item.Drop();
        //        Debug.Log("Your hands are full bish");
        //    } 
        //}


        //private void OnMouseUp()
        //{
        //    RaycastHit hit;
        //    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        //    if (Physics.Raycast(ray, out hit))
        //    {
        //        var clickedItem = hit.transform.GetComponent<iItem>();
        //        clickedItem.PickUp(GetComponent<Player>());
        //    }
        //}

    }

    public interface iEquippable<TItem>
        where TItem : iItem
    {
        TItem Equip(TItem item);
        TItem Remove(TItem item);
    }
}