using Sirenix.OdinInspector;
using Theia.IoC;
using Theia.Items;
using Theia.Items.Base;

namespace Theia.Stats.gear
{
    [HideReferenceObjectPicker]
    public class GearSlot : DataClient<GearSlotData>, iWearableItemSlot<InventoryItem>, iWeightProvider
    {
        [ListDrawerSettings(Expanded = true), ReadOnly]
        public InventoryItem[] slots;

        public InventoryItem Equip(InventoryItem newItem)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] == null)
                {
                    slots[i] = newItem;
                    return default;
                }                    
            }
            return newItem;
        }


        public InventoryItem Remove(InventoryItem item)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] == item)
                {
                    slots[i] = null;
                    return item;
                }
            }
            return null;
        }
        public int GetWeight() => utils.Sum<InventoryItem>(slots, item => item ? item.weight : 0);

        public override void Init(GearSlotData data)
        {
            base.Init(data);
            slots = new InventoryItem[data.numSlots];
        }
    }
}
