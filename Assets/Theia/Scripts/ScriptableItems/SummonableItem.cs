// summons a 'Summonable' entity type.
// not to be confused with Monster Scrolls, that simply spawn monsters.
// (summonables are entities that belong to the player, like pets and mounts)
using UnityEngine;
using Mirror;

public abstract class SummonableItem : UsableItem
{
    [Header("Summonable")]
    public Summonable summonPrefab;
    public long revivePrice = 10;
    public bool removeItemIfDied;

    // usage
    public override bool CanUse(Player player, int inventoryIndex)
    {
        // summon only if:
        //  summonable not dead (dead summonable item has to be revived first)
        //  not while fighting, trading, stunned, dead, etc
        //  player level at least summonable level to avoid power leveling
        //    with someone else's high level summonable
        //  -> also use riskyActionTime to avoid spamming. we don't want someone
        //     to spawn and destroy a pet 1000x/second
        Item item = player.inventory.slots[inventoryIndex].item;
        return base.CanUse(player, inventoryIndex) &&
               (player.state == "IDLE" || player.state == "MOVING") &&
               NetworkTime.time >= player.nextRiskyActionTime &&
               summonPrefab != null &&
               item.summonedHealth > 0 &&
               item.summonedLevel <= player.level.current;
    }

    public override void Use(Player player, int inventoryIndex)
    {
        // always call base function too
        base.Use(player, inventoryIndex);

        // set risky action time (1s should be okay)
        player.nextRiskyActionTime = NetworkTime.time + 1;
    }
}
