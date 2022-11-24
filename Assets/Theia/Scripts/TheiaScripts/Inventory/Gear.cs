using Items;
using Sirenix.OdinInspector;
using UnityEngine;

namespace InventoryStuff
{
    public class Gear : SerializedMonoBehaviour
    {
        [ReadOnly]
        public InventoryTemplate template;

        public HandSlot rightHand = new HandSlot();
        public HandSlot leftHand = new HandSlot();
        [ReadOnly] public GearSlots slots = new GearSlots();

        // TODO: will this method return/display a message for feedback?
        // TODO: may need a variation of this method to swap and/or automatically get and store an item in next available inventory
        public void GrabItem(IItem item)
        {
            // Try to get the item with right hand first
            if (rightHand.item == null)
            {
                rightHand.item = item;
                item.currentSlot = rightHand;
            }
                
            else if (leftHand.item == null)
            {
                leftHand.item = item;
                item.currentSlot = leftHand;
            }
            else
            {
                item.Drop();
                Debug.Log("Your hands are full bish");
            } 
        }

        public void StowItem(IItem item)
        {
            // find a suitable container to stow item in

        }

        private void OnValidate()
        {
            slots.PopulateSlotsFromTemplate(template);
        }

        private void OnMouseUp()
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                var clickedItem = hit.transform.GetComponent<IItem>();
                clickedItem.PickUp(GetComponent<Player>());
            }
        }
    }
}