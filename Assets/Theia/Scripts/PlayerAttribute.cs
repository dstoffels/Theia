// for attributes, but we can't call it 'Attribute' because of C#'s Attribute
using UnityEngine;
using Mirror;

[RequireComponent(typeof(Level))]
[RequireComponent(typeof(Health))]
public abstract class PlayerAttribute : NetworkBehaviour
{
    [Header("Components")]
    public Level level;
    public Health health;

    [Header("Attribute")]
    [SyncVar] public int value;
    public static int SpendablePerLevel = 2;

    // cache attribute components
    // (assigned when needed. NOT in Awake because then prefab.max doesn't work)
    PlayerAttribute[] _attributes;
    PlayerAttribute[] attributes =>
        _attributes ?? (_attributes = GetComponents<PlayerAttribute>());

    public int TotalPointsSpent()
    {
        // avoid Linq for performance / GC
        int spent = 0;
        foreach (PlayerAttribute attribute in attributes)
            spent += attribute.value;
        return spent;
    }

    public int PointsSpendable()
    {
        // calculate the amount of attribute points that can still be spent
        // -> 'AttributesSpendablePerLevel' points per level
        // -> we don't need to store the points in an extra variable, we can
        //    simply decrease the attribute points spent from the level
        // (avoid Linq for performance/GC)
        return (level.current * SpendablePerLevel) - TotalPointsSpent();
    }

    [Command]
    public void CmdIncrease()
    {
        // validate
        if (health.current > 0 && PointsSpendable() > 0)
            ++value;
    }
}
