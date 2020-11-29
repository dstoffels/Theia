using UnityEngine;

// inventory, attributes etc. can influence max mana
public interface IManaBonus
{
    int GetManaBonus(int baseMana);
    int GetManaRecoveryBonus();
}

[RequireComponent(typeof(Level))]
[DisallowMultipleComponent]
public class Mana : Energy
{
    public Level level;
    public LinearInt baseMana = new LinearInt{baseValue=100};
    public int baseRecoveryRate = 1;

    // cache components that give a bonus (attributes, inventory, etc.)
    // (assigned when needed. NOT in Awake because then prefab.max doesn't work)
    IManaBonus[] _bonusComponents;
    IManaBonus[] bonusComponents =>
        _bonusComponents ?? (_bonusComponents = GetComponents<IManaBonus>());

    // calculate max
    public override int max
    {
        get
        {
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            int bonus = 0;
            int baseThisLevel = baseMana.Get(level.current);
            foreach (IManaBonus bonusComponent in bonusComponents)
                bonus += bonusComponent.GetManaBonus(baseThisLevel);
            return baseThisLevel + bonus;
        }
    }

    public override int recoveryRate
    {
        get
        {
            // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
            int bonus = 0;
            foreach (IManaBonus bonusComponent in bonusComponents)
                bonus += bonusComponent.GetManaRecoveryBonus();
            return baseRecoveryRate + bonus;
        }
    }
}