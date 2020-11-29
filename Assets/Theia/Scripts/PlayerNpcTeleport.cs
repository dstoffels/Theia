using UnityEngine;
using Mirror;

[DisallowMultipleComponent]
public class PlayerNpcTeleport : NetworkBehaviour
{
    [Header("Components")]
    public Player player;

    [Command]
    public void CmdNpcTeleport()
    {
        // validate
        if (player.state == "IDLE" &&
            player.target != null &&
            player.target.health.current > 0 &&
            player.target is Npc npc &&
            Utils.ClosestDistance(player, npc) <= player.interactionRange &&
            npc.teleport.destination != null)
        {
            // using agent.Warp is recommended over transform.position
            // (the latter can cause weird bugs when using it with an agent)
            player.movement.Warp(npc.teleport.destination.position);

            // clear target. no reason to keep targeting the npc after we
            // teleported away from it
            player.target = null;
        }
    }
}
