using System.Collections.Generic;

namespace Theia.Stats.gear
{
    /// <summary>
    /// Custom Dictionary for the Inventory system.
    /// </summary>
    public class GearSlots : Dictionary<GearSlotData, GearSlot>
    {
        public void PopulateSlotsFromTemplate(InventoryTemplate template)
        {
            foreach (var slot in template.slots)
                if (!ContainsKey(slot))
                    Add(slot, new GearSlot());
            TakeOutTheTrash(template);
        }

        void TakeOutTheTrash(InventoryTemplate template)
        {
            foreach (var slot in Keys)
                if (!template.slots.Contains(slot))
                    Remove(slot);
        }
    }
}
