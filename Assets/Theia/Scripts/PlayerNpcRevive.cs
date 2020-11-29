using UnityEngine;
using Mirror;

[DisallowMultipleComponent]
public class PlayerNpcRevive : NetworkBehaviour
{
    [Header("Components")]
    public Player player;

    [Command]
    public void CmdRevive(int index)
    {
        // validate: close enough, npc alive and valid index and valid item?
        // use collider point(s) to also work with big entities
        if (player.state == "IDLE" &&
            player.target != null &&
            player.target.health.current > 0 &&
            player.target is Npc npc &&
            npc.revive != null && // only if Npc offers revive
            Utils.ClosestDistance(player, npc) <= player.interactionRange &&
            0 <= index && index < player.inventory.slots.Count)
        {
            ItemSlot slot = player.inventory.slots[index];
            if (slot.amount > 0 && slot.item.data is SummonableItem summonable)
            {
                // verify the pet status
                if (slot.item.summonedHealth == 0 && summonable.summonPrefab != null)
                {
                    // enough gold?
                    if (player.gold >= summonable.revivePrice)
                    {
                        // pay for it, revive it
                        player.gold -= summonable.revivePrice;
                        slot.item.summonedHealth = summonable.summonPrefab.health.max;
                        player.inventory.slots[index] = slot;
                    }
                }
            }
        }
    }
}
