// summonable entity types that are bound to a player (pet, mount, ...)
using Mirror;
using UnityEngine;

public abstract class Summonable : EntityOLD
{
    [SyncVar, HideInInspector] public PlayerOLD owner;

    // sync with owner's item //////////////////////////////////////////////////
    protected virtual ItemSlot SyncStateToItemSlot(ItemSlot slot)
    {
        slot.item.summonedHealth = health.current;
        slot.item.summonedLevel = level.current;

        // remove item if died?
        if (((SummonableItem)slot.item.data).removeItemIfDied && health.current == 0)
            --slot.amount;

        return slot;
    }

    // find owner item index
    // (avoid FindIndex for performance/GC)
    public int GetOwnerItemIndex()
    {
        if (owner != null)
        {
            for (int i = 0; i < owner.inventory.slots.Count; ++i)
            {
                ItemSlot slot = owner.inventory.slots[i];
                if (slot.amount > 0 && slot.item.summoned == netIdentity)
                    return i;
            }
        }
        return -1;
    }

    // to save computations we don't sync to it all the time, it's enough to
    // sync in:
    // * OnDestroy when unsummoning the pet
    // * On experience gain so that level ups and exp are saved properly
    // * OnDeath so that people can't cheat around reviving pets
    // => after a server crash the health/mana might not be exact, but that's a
    //    good price to pay to save computations in each Update tick
    [Server]
    public void SyncToOwnerItem()
    {
        // owner might be null if server shuts down and owner was destroyed before
        if (owner != null)
        {
            // find the item (amount might be 0 already if a mount died, etc.)
            int index = GetOwnerItemIndex();
            if (index != -1)
                owner.inventory.slots[index] = SyncStateToItemSlot(owner.inventory.slots[index]);
        }
    }
}
