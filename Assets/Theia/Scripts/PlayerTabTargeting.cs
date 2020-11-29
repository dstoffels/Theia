using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(Player))]
[RequireComponent(typeof(PlayerIndicator))]
[DisallowMultipleComponent]
public class PlayerTabTargeting : NetworkBehaviour
{
    [Header("Components")]
    public Player player;
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
        // note: uses Linq, but this only happens on the client when pressing Tab
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Monster");
        List<Monster> monsters = objects.Select(go => go.GetComponent<Monster>()).Where(m => m.health.current > 0).ToList();
        List<Monster> sorted = monsters.OrderBy(m => Vector3.Distance(transform.position, m.transform.position)).ToList();

        // target nearest one
        if (sorted.Count > 0)
        {
            indicator.SetViaParent(sorted[0].transform);
            player.CmdSetTarget(sorted[0].netIdentity);
        }
    }
}
