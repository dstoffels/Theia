using Sirenix.OdinInspector;
using UnityEngine;
using Theia.Items.Base;

namespace Theia.Stats.gear
{
    public class Gear : DataClientManager<GearSlotData, GearSlot>, iEquippable<GearItem>
    {
        [Button]
        public GearItem Equip(GearItem item)
        {
            return this[item.slot].Equip(item);
        }

        [Button]
        public GearItem Remove(GearItem item)
        {
            return this[item.slot].Remove(item);
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

        //public void StowItem(iItem item)
        //{
        //    // find a suitable container to stow item in

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
        where TItem : Items.Base.iItem
    {
        TItem Equip(TItem item);
        TItem Remove(TItem item);
    }
}