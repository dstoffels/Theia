using Sirenix.OdinInspector;
using Items.Armor;
using Items;
using Stats;

namespace InventoryStuff.Armor
{
    [HideReferenceObjectPicker]
    public class ArmorSlot : BaseStat<ArmorSlotData>, IWearableItemSlot
    {
        const float width = 75;
        
        [LabelWidth(width)] public PaddedArmor padded;
        [LabelWidth(width)] public MailleArmor maille;
        [LabelWidth(width)] public HeavyArmor heavy;

        public float slotHindrance => GetSlotHindrance();
        public float slotWeight => GetSlotWeight();

        private float GetSlotHindrance()
        {
            float total = 0;
            total += padded is null ? 0 : padded.hindrance;
            total += maille is null ? 0 : maille.hindrance;
            total += heavy is null ? 0 : heavy.hindrance;
            return total;
        }

        private float GetSlotWeight()
        {
            float total = 0;
            total += padded is null ? 0 : padded.weight;
            total += maille is null ? 0 : maille.weight;
            total += heavy is null ? 0 : heavy.weight;
            return total;
        }

        // triggered by an item currently worn in padded, maille or heavy.
        public IItem RemoveItem(IItem item)
        {
            // clear appropriate slot
            padded = item is PaddedArmor ? null : padded;
            maille = item is MailleArmor ? null : maille;
            heavy = item is HeavyArmor ? null : heavy;
            return item;
        }

        // triggered by an item
        public IItem WearItem(IItem newItem)
        {
            IItem curItem = null;
            if(newItem is PaddedArmor)
            {
                curItem = RemoveItem(padded);
                padded = (PaddedArmor)newItem;
            }

            if(newItem is MailleArmor)
            {
                curItem = RemoveItem(maille);
                maille = (MailleArmor)newItem;
            }

            if(newItem is HeavyArmor)
            {
                curItem = RemoveItem(heavy);
                heavy = (HeavyArmor)newItem;
            }

            return curItem;
        }
    }
}