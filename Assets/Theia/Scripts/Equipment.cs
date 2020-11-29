using UnityEngine;

[DisallowMultipleComponent]
public abstract class Equipment : ItemContainer, IHealthBonus, IManaBonus, ICombatBonus
{
    // boni ////////////////////////////////////////////////////////////////////
    public int GetHealthBonus(int baseHealth)
    {
        // calculate equipment bonus
        // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
        int bonus = 0;
        foreach (ItemSlot slot in slots)
            if (slot.amount > 0 && slot.item.CheckDurability())
                bonus += ((EquipmentItem)slot.item.data).healthBonus;
        return bonus;
    }

    public int GetHealthRecoveryBonus() => 0;

    public int GetManaBonus(int baseMana)
    {
        // calculate equipment bonus
        // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
        int bonus = 0;
        foreach (ItemSlot slot in slots)
            if (slot.amount > 0 && slot.item.CheckDurability())
                bonus += ((EquipmentItem)slot.item.data).manaBonus;
        return bonus;
    }

    public int GetManaRecoveryBonus() => 0;

    public int GetDamageBonus()
    {
        // calculate equipment bonus
        // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
        int bonus = 0;
        foreach (ItemSlot slot in slots)
            if (slot.amount > 0 && slot.item.CheckDurability())
                bonus += ((EquipmentItem)slot.item.data).damageBonus;
        return bonus;
    }

    public int GetDefenseBonus()
    {
        // calculate equipment bonus
        // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
        int bonus = 0;
        foreach (ItemSlot slot in slots)
            if (slot.amount > 0 && slot.item.CheckDurability())
                bonus += ((EquipmentItem)slot.item.data).defenseBonus;
        return bonus;
    }

    public float GetCriticalChanceBonus()
    {
        // calculate equipment bonus
        // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
        float bonus = 0;
        foreach (ItemSlot slot in slots)
            if (slot.amount > 0 && slot.item.CheckDurability())
                bonus += ((EquipmentItem)slot.item.data).criticalChanceBonus;
        return bonus;
    }

    public float GetBlockChanceBonus()
    {
        // calculate equipment bonus
        // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
        float bonus = 0;
        foreach (ItemSlot slot in slots)
            if (slot.amount > 0 && slot.item.CheckDurability())
                bonus += ((EquipmentItem)slot.item.data).blockChanceBonus;
        return bonus;
    }

    ////////////////////////////////////////////////////////////////////////////
    // helper function to find the equipped weapon index
    // -> works for all entity types. returns -1 if no weapon equipped.
    public int GetEquippedWeaponIndex()
    {
        // (avoid FindIndex to minimize allocations)
        for (int i = 0; i < slots.Count; ++i)
        {
            ItemSlot slot = slots[i];
            if (slot.amount > 0 && slot.item.data is WeaponItem)
                return i;
        }
        return -1;
    }

    // get currently equipped weapon category to check if skills can be casted
    // with this weapon. returns "" if none.
    public string GetEquippedWeaponCategory()
    {
        // find the weapon slot
        int index = GetEquippedWeaponIndex();
        return index != -1 ? ((WeaponItem)slots[index].item.data).category : "";
    }
}
