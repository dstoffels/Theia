using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace InventoryStuff.Armor
{
    public class ArmorSlots : Dictionary<ArmorSlotData, ArmorSlot>
    {
        [Button]
        public void PopulateSlotsFromTemplate(ArmorSlotTemplate template)
        {
            foreach (var slot in template.slots)
                if (!ContainsKey(slot))
                    Add(slot, new ArmorSlot());
            TakeOutTheTrash(template);
        }

        void TakeOutTheTrash(ArmorSlotTemplate template)
        {
            foreach (var slot in Keys)
                if (!template.slots.Contains(slot))
                    Remove(slot);
        }
    }
}