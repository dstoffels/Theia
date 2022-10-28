using Sirenix.OdinInspector;
using Items;

namespace InventoryStuff
{
    [HideReferenceObjectPicker, InlineProperty]
    public abstract class ItemSlot<Item> : IWearableItemSlot
        where Item : IItem
    {
        [LabelWidth(75)]
        public Item wornItem;

        public IItem RemoveItem(IItem item)
        {
            var temp = item;
            wornItem = default;
            return temp;
        }

        public IItem WearItem(IItem newItem)
        {
            var temp = wornItem;

            wornItem = (Item)newItem;
            wornItem.currentSlot = this;

            if (temp == null)
                return null;
            else 
                return temp;
        }
    }
}
