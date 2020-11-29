// npc brain does nothing but stand around
using UnityEngine;
using Mirror;

[CreateAssetMenu(menuName="uMMORPG Brain/Brains/Mount", order=999)]
public class MountBrain : CommonBrain
{
    // events //////////////////////////////////////////////////////////////////
    public bool EventOwnerDied(Summonable summonable) =>
        summonable.owner != null && summonable.owner.health.current == 0;

    public bool EventOwnerDisappeared(Summonable summonable) =>
        summonable.owner == null;

    public bool EventDeathTimeElapsed(Mount mount) =>
        mount.state == "DEAD" && NetworkTime.time >= mount.deathTimeEnd;

    // states //////////////////////////////////////////////////////////////////
    // copy owner's position and rotation. no need for NetworkTransform.
    void CopyOwnerPositionAndRotation(Mount mount)
    {
        if (mount.owner != null)
        {
            // mount doesn't need a movement system with a .Warp function.
            // we simply copy the owner position/rotation at all times.
            mount.transform.position = mount.owner.transform.position;
            mount.transform.rotation = mount.owner.transform.rotation;
        }
    }

    string UpdateServer_IDLE(Mount mount)
    {
        // copy owner's position and rotation. no need for NetworkTransform.
        CopyOwnerPositionAndRotation(mount);

        // events sorted by priority (e.g. target doesn't matter if we died)
        if (EventOwnerDisappeared(mount))
        {
            // owner might disconnect or get destroyed for some reason
            NetworkServer.Destroy(mount.gameObject);
            return "IDLE";
        }
        if (EventOwnerDied(mount))
        {
            // die if owner died, so the mount doesn't stand around there forever
            mount.health.current = 0;
        }
        if (EventDied(mount))
        {
            // we died.
            return "DEAD";
        }
        if (EventDeathTimeElapsed(mount)) {} // don't care

        return "IDLE"; // nothing interesting happened
    }

    string UpdateServer_DEAD(Mount mount)
    {
        // events sorted by priority (e.g. target doesn't matter if we died)
        if (EventOwnerDisappeared(mount))
        {
            // owner might disconnect or get destroyed for some reason
            NetworkServer.Destroy(mount.gameObject);
            return "DEAD";
        }
        if (EventDeathTimeElapsed(mount))
        {
            // we were lying around dead for long enough now.
            // hide while respawning, or disappear forever
            NetworkServer.Destroy(mount.gameObject);
            return "DEAD";
        }
        if (EventOwnerDied(mount)) {} // don't care
        if (EventDied(mount)) {} // don't care, of course we are dead

        return "DEAD"; // nothing interesting happened
    }

    public override string UpdateServer(Entity entity)
    {
        Mount mount = (Mount)entity;

        if (mount.state == "IDLE") return UpdateServer_IDLE(mount);
        if (mount.state == "DEAD") return UpdateServer_DEAD(mount);

        Debug.LogError("invalid state:" + mount.state);
        return "IDLE";
    }

    public override void UpdateClient(Entity entity)
    {
        Mount mount = (Mount)entity;
        if (mount.state == "IDLE" || mount.state == "MOVING")
        {
            // copy owner's position and rotation. no need for NetworkTransform.
            CopyOwnerPositionAndRotation(mount);
        }
    }
}
