using Sirenix.OdinInspector;
using Items;

namespace InventoryStuff
{
    [HideReferenceObjectPicker, InlineProperty]
    public abstract class ItemSlot<Item> : IWearableItemSlot
        where Item : iItem
    {
        [LabelWidth(75)]
        public Item item;

        public iItem RemoveItem(iItem item)
        {
            var temp = item;
            this.item = default;
            return temp;
        }

        public iItem WearItem(iItem newItem)
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
