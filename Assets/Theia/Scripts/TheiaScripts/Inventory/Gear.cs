using Sirenix.OdinInspector;
using UnityEngine;
using Theia.Items.Base;

namespace Theia.Stats.gear
{
    public class Gear : DataClientManager<GearSlotData, GearSlot>, iEquippable<InventoryItem>
    {
        [ShowInInspector]
        public int totalWeight { get; private set; }

        public void SetTotalWeight() => utils.Sum<GearSlot>(all, slot => slot.GetWeight());

        [Button]
        public InventoryItem Equip(InventoryItem item)
        {
            item = this[item.slot].Equip(item);
            SetTotalWeight();
            return item;
        }

        [Button]
        public InventoryItem Remove(InventoryItem item)
        {
            item = this[item.slot].Remove(item);
            SetTotalWeight();
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