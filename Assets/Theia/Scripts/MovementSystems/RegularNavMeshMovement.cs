// navmesh movement for monsters, pets, etc.
// -> uses NetworkNavMeshAgent instead of Rubberbanding version!
using UnityEngine;

[RequireComponent(typeof(NetworkNavMeshAgent))]
[DisallowMultipleComponent]
public class RegularNavMeshMovement : NavMeshMovement
{
    [Header("Components")]
    public NetworkNavMeshAgent networkNavMeshAgent;

    public override void Reset()
    {
        agent.ResetMovement();
    }

    // for 4 years since uMMORPG release we tried to detect warps in
    // NetworkNavMeshAgent/Rubberbanding. it never worked 100% of the time:
    // -> checking if dist(pos, lastpos) > speed worked well for far teleports,
    //    but failed for near teleports with dist < speed meters.
    // -> checking if speed since last update is > speed is the perfect idea,
    //    but it turns out that NavMeshAgent sometimes moves faster than
    //    agent.speed, e.g. when moving up or down a corner/stone. in fact, it
    //    sometimes moves up to 5x faster than speed, which makes warp detection
    //    hard.
    // => the ONLY 100% RELIABLE solution is to have our own Warp function that
    //    force warps the client over the network.
    // => this is extremely important for cases where players get warped behind
    //    a small door or wall. this just has to work.
    public override void Warp(Vector3 destination)
    {
        // NetworkNavMeshAgent needs to know about warp. this is the only 100%
        // reliable way to detect it.
        if (isServer)
            networkNavMeshAgent.RpcWarp(destination);
        agent.Warp(destination);
    }
}
