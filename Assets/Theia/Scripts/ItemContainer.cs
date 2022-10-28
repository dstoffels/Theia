// Inventory & Equip both use slots and some common functions. might as well
// abstract them to save code.
using Mirror;

public abstract class ItemContainer : NetworkBehaviour
{
    // the slots
    public SyncList<ItemSlot> slots = new SyncList<ItemSlot>();

    // helper function to find an item in the slots
    public int GetItemIndexByName(string itemName)
    {
        // (avoid FindIndex to minimize allocations)
        for (int i = 0; i < slots.Count; ++i)
        {
            ItemSlot slot = slots[i];
            if (slot.amount > 0 && slot.item.name == itemName)
                return i;
        }
        return -1;
    }

    // durability //////////////////////////////////////////////////////////////
    // calculate total missing durability
    public int GetTotalMissingDurability()
    {
        int total = 0;
        foreach (ItemSlot slot in slots)
            if (slot.amount > 0 && slot.item.data.maxDurability > 0)
                total += slot.item.data.maxDurability - slot.item.durability;
        return total;
    }

    // repair all items. can be used by Npc.
    [Server]
    public void RepairAllItems()
    {
        for (int i = 0; i < slots.Count; ++i)
        {
            if (slots[i].amount > 0)
            {
                ItemSlot slot = slots[i];
                slot.item.durability = slot.item.maxDurability;
                slots[i] = slot;
            }
        }
    }
}
