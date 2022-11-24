using Sirenix.OdinInspector;
using Items;

namespace InventoryStuff
{
    [HideReferenceObjectPicker, InlineProperty]
    public abstract class ItemSlot<Item> : IWearableItemSlot
        where Item : IItem
    {
        [LabelWidth(75)]
        public Item item;

        public IItem RemoveItem(IItem item)
        {
            var temp = item;
            this.item = default;
            return temp;
        }

        public IItem WearItem(IItem newItem)
        {
            var temp = item;

            item = (Item)newItem;
            item.currentSlot = this;

            if (temp == null)
                return null;
            else 
                return temp;
        }
    }
}
