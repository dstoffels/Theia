using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(PlayerOLD))]
[RequireComponent(typeof(PlayerIndicator))]
[DisallowMultipleComponent]
public class PlayerTabTargeting : NetworkBehaviour
{
    [Header("Components")]
    public PlayerOLD player;
    public PlayerIndicator indicator;

    [Header("Targeting")]
    public KeyCode key = KeyCode.Tab;

    void Update()
    {
        // only for local player
        if (!isLocalPlayer) return;

        // in a state where tab targeting is allowed?
        if (player.state == "IDLE" ||
            player.state == "MOVING" ||
            player.state == "CASTING" ||
            player.state == "STUNNED")
        {
            // key pressed?
            if (Input.GetKeyDown(key))
                TargetNearest();
        }
    }

    [Client]
    void TargetNearest()
    {
        // find all monsters that are alive, sort by distance
        // (NetworkIdentity.spawned is available on client too for those it sees)
        // note: uses Linq, but this only happens on the client when pressing Tab
        List<Monster> monsters = NetworkIdentity.spawned.Values
            .Select(ni => ni.GetComponent<Monster>())
            .Where(m => m != null && m.health.current > 0)
            .ToList();
        List<Monster> sorted = monsters.OrderBy(m => Vector3.Distance(transform.position, m.transform.position)).ToList();

        // target nearest one
        if (sorted.Count > 0)
        {
            indicator.SetViaParent(sorted[0].transform);
            player.CmdSetTarget(sorted[0].netIdentity);
        }
    }
}
