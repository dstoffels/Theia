using UnityEngine;
using Mirror;

[CreateAssetMenu(menuName="uMMORPG Brain/Brains/Pet", order=999)]
public class PetBrain : CommonBrain
{
    [Header("Movement")]
    public float returnDistance = 25; // return to player if dist > ...
    // pets should follow their targets even if they run out of the movement
    // radius. the follow dist should always be bigger than the biggest archer's
    // attack range, so that archers will always pull aggro, even when attacking
    // from far away.
    public float followDistance = 20;
    // pet should teleport if the owner gets too far away for whatever reason
    public float teleportDistance = 30;
    [Range(0.1f, 1)] public float attackToMoveRangeRatio = 0.8f; // move as close as 0.8 * attackRange to a target

    // events //////////////////////////////////////////////////////////////////
    public bool EventOwnerDisappeared(Summonable summonable) =>
        summonable.owner == null;

    public bool EventDeathTimeElapsed(Pet pet) =>
        pet.state == "DEAD" && NetworkTime.time >= pet.deathTimeEnd;

    public bool EventNeedReturnToOwner(Pet pet) =>
        Vector3.Distance(pet.owner.petControl.petDestination, pet.transform.position) > returnDistance;

    public bool EventNeedTeleportToOwner(Pet pet) =>
        Vector3.Distance(pet.owner.petControl.petDestination, pet.transform.position) > teleportDistance;

    public bool EventTargetTooFarToFollow(Pet pet) =>
        pet.target != null &&
        Vector3.Distance(pet.owner.petControl.petDestination, Utils.ClosestPoint(pet.target, pet.transform.position)) > followDistance;

    // states //////////////////////////////////////////////////////////////////
    string UpdateServer_IDLE(Pet pet)
    {
        // events sorted by priority (e.g. target doesn't matter if we died)
        if (EventOwnerDisappeared(pet))
        {
            // owner might disconnect or get destroyed for some reason
            NetworkServer.Destroy(pet.gameObject);
            return "IDLE";
        }
        if (EventDied(pet))
        {
            // we died.
            return "DEAD";
        }
        if (EventStunned(pet))
        {
            pet.movement.Reset();
            return "STUNNED";
        }
        if (EventTargetDied(pet))
        {
            // we had a target before, but it died now. clear it.
            pet.target = null;
            pet.skills.CancelCast();
            return "IDLE";
        }
        if (EventNeedTeleportToOwner(pet))
        {
            pet.movement.Warp(pet.owner.petControl.petDestination);
            return "IDLE";
        }
        if (EventNeedReturnToOwner(pet))
        {
            // return to owner only while IDLE
            pet.target = null;
            pet.skills.CancelCast();
            pet.movement.Navigate(pet.owner.petControl.petDestination, 0);
            return "MOVING";
        }
        if (EventTargetTooFarToFollow(pet))
        {
            // we had a target before, but it's out of follow range now.
            // clear it and go back to start. don't stay here.
            pet.target = null;
            pet.skills.CancelCast();
            pet.movement.Navigate(pet.owner.petControl.petDestination, 0);
            return "MOVING";
        }
        if (EventTargetTooFarToAttack(pet))
        {
            // we had a target before, but it's out of attack range now.
            // follow it. (use collider point(s) to also work with big entities)
            float stoppingDistance = ((PetSkills)pet.skills).CurrentCastRange() * attackToMoveRangeRatio;
            Vector3 destination = Utils.ClosestPoint(pet.target, pet.transform.position);
            pet.movement.Navigate(destination, stoppingDistance);
            return "MOVING";
        }
        if (EventSkillRequest(pet))
        {
            // we had a target in attack range before and trying to cast a skill
            // on it. check self (alive, mana, weapon etc.) and target
            Skill skill = pet.skills.skills[pet.skills.currentSkill];
            if (pet.skills.CastCheckSelf(skill) && pet.skills.CastCheckTarget(skill))
            {
                // start casting
                pet.skills.StartCast(skill);
                return "CASTING";
            }
            else
            {
                // invalid target. reset attempted current skill cast.
                pet.target = null;
                pet.skills.currentSkill = -1;
                return "IDLE";
            }
        }
        if (EventAggro(pet))
        {
            // target in attack range. try to cast a first skill on it
            if (pet.skills.skills.Count > 0) pet.skills.currentSkill = ((PetSkills)pet.skills).NextSkill();
            else Debug.LogError(name + " has no skills to attack with.");
            return "IDLE";
        }
        if (EventMoveEnd(pet)) {} // don't care
        if (EventDeathTimeElapsed(pet)) {} // don't care
        if (EventSkillFinished(pet)) {} // don't care
        if (EventTargetDisappeared(pet)) {} // don't care

        return "IDLE"; // nothing interesting happened
    }

    string UpdateServer_MOVING(Pet pet)
    {
        // events sorted by priority (e.g. target doesn't matter if we died)
        if (EventOwnerDisappeared(pet))
        {
            // owner might disconnect or get destroyed for some reason
            NetworkServer.Destroy(pet.gameObject);
            return "IDLE";
        }
        if (EventDied(pet))
        {
            // we died.
            pet.movement.Reset();
            return "DEAD";
        }
        if (EventStunned(pet))
        {
            pet.movement.Reset();
            return "STUNNED";
        }
        if (EventMoveEnd(pet))
        {
            // we reached our destination.
            return "IDLE";
        }
        if (EventTargetDied(pet))
        {
            // we had a target before, but it died now. clear it.
            pet.target = null;
            pet.skills.CancelCast();
            pet.movement.Reset();
            return "IDLE";
        }
        if (EventNeedTeleportToOwner(pet))
        {
            pet.movement.Warp(pet.owner.petControl.petDestination);
            return "IDLE";
        }
        if (EventTargetTooFarToFollow(pet))
        {
            // we had a target before, but it's out of follow range now.
            // clear it and go back to start. don't stay here.
            pet.target = null;
            pet.skills.CancelCast();
            pet.movement.Navigate(pet.owner.petControl.petDestination, 0);
            return "MOVING";
        }
        if (EventTargetTooFarToAttack(pet))
        {
            // we had a target before, but it's out of attack range now.
            // follow it. (use collider point(s) to also work with big entities)
            float stoppingDistance = ((PetSkills)pet.skills).CurrentCastRange() * attackToMoveRangeRatio;
            Vector3 destination = Utils.ClosestPoint(pet.target, pet.transform.position);
            pet.movement.Navigate(destination, stoppingDistance);
            return "MOVING";
        }
        if (EventAggro(pet))
        {
            // target in attack range. try to cast a first skill on it
            // (we may get a target while randomly wandering around)
            if (pet.skills.skills.Count > 0) pet.skills.currentSkill = ((PetSkills)pet.skills).NextSkill();
            else Debug.LogError(name + " has no skills to attack with.");
            pet.movement.Reset();
            return "IDLE";
        }
        if (EventNeedReturnToOwner(pet)) {} // don't care
        if (EventDeathTimeElapsed(pet)) {} // don't care
        if (EventSkillFinished(pet)) {} // don't care
        if (EventTargetDisappeared(pet)) {} // don't care
        if (EventSkillRequest(pet)) {} // don't care, finish movement first

        return "MOVING"; // nothing interesting happened
    }

    string UpdateServer_CASTING(Pet pet)
    {
        // keep looking at the target for server & clients (only Y rotation)
        if (pet.target)
            pet.movement.LookAtY(pet.target.transform.position);

        // events sorted by priority (e.g. target doesn't matter if we died)
        if (EventOwnerDisappeared(pet))
        {
            // owner might disconnect or get destroyed for some reason
            NetworkServer.Destroy(pet.gameObject);
            return "IDLE";
        }
        if (EventDied(pet))
        {
            // we died.
            return "DEAD";
        }
        if (EventStunned(pet))
        {
            pet.skills.CancelCast();
            pet.movement.Reset();
            return "STUNNED";
        }
        if (EventTargetDisappeared(pet))
        {
            // cancel if the target matters for this skill
            if (pet.skills.skills[pet.skills.currentSkill].cancelCastIfTargetDied)
            {
                pet.skills.CancelCast();
                pet.target = null;
                return "IDLE";
            }
        }
        if (EventTargetDied(pet))
        {
            // cancel if the target matters for this skill
            if (pet.skills.skills[pet.skills.currentSkill].cancelCastIfTargetDied)
            {
                pet.skills.CancelCast();
                pet.target = null;
                return "IDLE";
            }
        }
        if (EventSkillFinished(pet))
        {
            // finished casting. apply the skill on the target.
            pet.skills.FinishCast(pet.skills.skills[pet.skills.currentSkill]);

            // did the target die? then clear it so that the monster doesn't
            // run towards it if the target respawned
            if (pet.target.health.current == 0) pet.target = null;

            // go back to IDLE. reset current skill.
            ((PetSkills)pet.skills).lastSkill = pet.skills.currentSkill;
            pet.skills.currentSkill = -1;
            return "IDLE";
        }
        if (EventMoveEnd(pet)) {} // don't care
        if (EventDeathTimeElapsed(pet)) {} // don't care
        if (EventNeedTeleportToOwner(pet)) {} // don't care
        if (EventNeedReturnToOwner(pet)) {} // don't care
        if (EventTargetTooFarToAttack(pet)) {} // don't care, we were close enough when starting to cast
        if (EventTargetTooFarToFollow(pet)) {} // don't care, we were close enough when starting to cast
        if (EventAggro(pet)) {} // don't care, always have aggro while casting
        if (EventSkillRequest(pet)) {} // don't care, that's why we are here

        return "CASTING"; // nothing interesting happened
    }

    string UpdateServer_STUNNED(Pet pet)
    {
        // events sorted by priority (e.g. target doesn't matter if we died)
        if (EventOwnerDisappeared(pet))
        {
            // owner might disconnect or get destroyed for some reason
            NetworkServer.Destroy(pet.gameObject);
            return "IDLE";
        }
        if (EventDied(pet))
        {
            // we died.
            pet.skills.CancelCast(); // in case we died while trying to cast
            return "DEAD";
        }
        if (EventStunned(pet))
        {
            return "STUNNED";
        }

        // go back to idle if we aren't stunned anymore and process all new
        // events there too
        return "IDLE";
    }

    string UpdateServer_DEAD(Pet pet)
    {
        // events sorted by priority (e.g. target doesn't matter if we died)
        if (EventOwnerDisappeared(pet))
        {
            // owner might disconnect or get destroyed for some reason
            NetworkServer.Destroy(pet.gameObject);
            return "DEAD";
        }
        if (EventDeathTimeElapsed(pet))
        {
            // we were lying around dead for long enough now.
            // hide while respawning, or disappear forever
            NetworkServer.Destroy(pet.gameObject);
            return "DEAD";
        }
        if (EventSkillRequest(pet)) {} // don't care
        if (EventSkillFinished(pet)) {} // don't care
        if (EventMoveEnd(pet)) {} // don't care
        if (EventNeedTeleportToOwner(pet)) {} // don't care
        if (EventNeedReturnToOwner(pet)) {} // don't care
        if (EventTargetDisappeared(pet)) {} // don't care
        if (EventTargetDied(pet)) {} // don't care
        if (EventTargetTooFarToFollow(pet)) {} // don't care
        if (EventTargetTooFarToAttack(pet)) {} // don't care
        if (EventAggro(pet)) {} // don't care
        if (EventDied(pet)) {} // don't care, of course we are dead

        return "DEAD"; // nothing interesting happened
    }

    public override string UpdateServer(Entity entity)
    {
        Pet pet = (Pet)entity;

        if (pet.state == "IDLE")    return UpdateServer_IDLE(pet);
        if (pet.state == "MOVING")  return UpdateServer_MOVING(pet);
        if (pet.state == "CASTING") return UpdateServer_CASTING(pet);
        if (pet.state == "STUNNED") return UpdateServer_STUNNED(pet);
        if (pet.state == "DEAD")    return UpdateServer_DEAD(pet);

        Debug.LogError("invalid state:" + pet.state);
        return "IDLE";
    }

    public override void UpdateClient(Entity entity)
    {
        Pet pet = (Pet)entity;
        if (pet.state == "CASTING")
        {
            // keep looking at the target for server & clients (only Y rotation)
            if (pet.target)
                pet.movement.LookAtY(pet.target.transform.position);
        }
    }

    // DrawGizmos can be used for debug info
    public override void DrawGizmos(Entity entity)
    {
        Pet pet = (Pet)entity;

        // draw the movement area (around 'start' if game running,
        // or around current position if still editing)
        Vector3 startHelp = Application.isPlaying ? pet.owner.petControl.petDestination : pet.transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(startHelp, returnDistance);

        // draw the follow dist
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(startHelp, followDistance);
    }
}
