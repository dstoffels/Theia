using Sirenix.OdinInspector;
using Theia.Items.Base;

namespace Theia.Stats.gear
{
    [HideReferenceObjectPicker, InlineProperty]
    public abstract class ItemSlot<TItem> : DataClient<GearSlotData>, iWearableItemSlot<TItem>
        where TItem : iItem
    {
        [LabelWidth(75)]
        public TItem item;

        public TItem Remove(TItem item)
        {
            var temp = item;
            this.item = default;
            return temp;
        }

        public TItem Equip(TItem newItem)
        {
            var temp = item;

            item = newItem;
            //item.currentSlot = this;

            if (temp == null)
                return default;
            else 
                return temp;
        }
    }
}
