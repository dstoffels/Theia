using UnityEngine;
using Mirror;

[CreateAssetMenu(menuName="uMMORPG Brain/Brains/Monster", order=999)]
public class MonsterBrain : CommonBrain
{
    [Header("Movement")]
    [Range(0, 1)] public float moveProbability = 0.1f; // chance per second
    public float moveDistance = 10;
    // monsters should follow their targets even if they run out of the movement
    // radius. the follow dist should always be bigger than the biggest archer's
    // attack range, so that archers will always pull aggro, even when attacking
    // from far away.
    public float followDistance = 20;
    [Range(0.1f, 1)] public float attackToMoveRangeRatio = 0.8f; // move as close as 0.8 * attackRange to a target

    // events //////////////////////////////////////////////////////////////////
    public bool EventDeathTimeElapsed(Monster monster) =>
        monster.state == "DEAD" && NetworkTime.time >= monster.deathTimeEnd;

    public bool EventMoveRandomly(Monster monster) =>
        Random.value <= moveProbability * Time.deltaTime;

    public bool EventRespawnTimeElapsed(Monster monster) =>
        monster.state == "DEAD" && monster.respawn && NetworkTime.time >= monster.respawnTimeEnd;

    public bool EventTargetTooFarToFollow(Monster monster) =>
        monster.target != null &&
        Vector3.Distance(monster.startPosition, Utils.ClosestPoint(monster.target, monster.transform.position)) > followDistance;

    // states //////////////////////////////////////////////////////////////////
    string UpdateServer_IDLE(Monster monster)
    {
        // events sorted by priority (e.g. target doesn't matter if we died)
        if (EventDied(monster))
        {
            // we died.
            return "DEAD";
        }
        if (EventStunned(monster))
        {
            monster.movement.Reset();
            return "STUNNED";
        }
        if (EventTargetDied(monster))
        {
            // we had a target before, but it died now. clear it.
            monster.target = null;
            monster.skills.CancelCast();
            return "IDLE";
        }
        if (EventTargetTooFarToFollow(monster))
        {
            // we had a target before, but it's out of follow range now.
            // clear it and go back to start. don't stay here.
            monster.target = null;
            monster.skills.CancelCast();
            monster.movement.Navigate(monster.startPosition, 0);
            return "MOVING";
        }
        if (EventTargetTooFarToAttack(monster))
        {
            // we had a target before, but it's out of attack range now.
            // follow it. (use collider point(s) to also work with big entities)
            float stoppingDistance = ((MonsterSkills)monster.skills).CurrentCastRange() * attackToMoveRangeRatio;
            Vector3 destination = Utils.ClosestPoint(monster.target, monster.transform.position);
            monster.movement.Navigate(destination, stoppingDistance);
            return "MOVING";
        }
        if (EventTargetEnteredSafeZone(monster))
        {
            // if our target entered the safe zone, we need to be really careful
            // to avoid kiting.
            // -> players could pull a monster near a safe zone and then step in
            //    and out of it before/after attacks without ever getting hit by
            //    the monster
            // -> running back to start won't help, can still kit while running
            // -> warping back to start won't help, we might accidentally placed
            //    a monster in attack range of a safe zone
            // -> the 100% secure way is to die and hide it immediately. many
            //    popular MMOs do it the same way to avoid exploits.
            // => call Entity.OnDeath without rewards etc. and hide immediately
            monster.OnDeath(); // no looting
            monster.respawnTimeEnd = NetworkTime.time + monster.respawnTime; // respawn in a while
            return "DEAD";
        }
        if (EventSkillRequest(monster))
        {
            // we had a target in attack range before and trying to cast a skill
            // on it. check self (alive, mana, weapon etc.) and target
            Skill skill = monster.skills.skills[monster.skills.currentSkill];
            if (monster.skills.CastCheckSelf(skill))
            {
                if (monster.skills.CastCheckTarget(skill))
                {
                    // start casting
                    monster.skills.StartCast(skill);
                    return "CASTING";
                }
                else
                {
                    // invalid target. clear the attempted current skill.
                    monster.target = null;
                    monster.skills.currentSkill = -1;
                    return "IDLE";
                }
            }
            else
            {
                // we can't cast this skill at the moment (cooldown/low mana/...)
                // -> clear the attempted current skill, but keep the target to
                // continue later
                monster.skills.currentSkill = -1;
                return "IDLE";
            }
        }
        if (EventAggro(monster))
        {
            // target in attack range. try to cast a first skill on it
            if (monster.skills.skills.Count > 0) monster.skills.currentSkill = ((MonsterSkills)monster.skills).NextSkill();
            else Debug.LogError(name + " has no skills to attack with.");
            return "IDLE";
        }
        if (EventMoveRandomly(monster))
        {
            // walk to a random position in movement radius (from 'start')
            // note: circle y is 0 because we add it to start.y
            Vector2 circle2D = Random.insideUnitCircle * moveDistance;
            monster.movement.Navigate(monster.startPosition + new Vector3(circle2D.x, 0, circle2D.y), 0);
            return "MOVING";
        }
        if (EventDeathTimeElapsed(monster)) {} // don't care
        if (EventRespawnTimeElapsed(monster)) {} // don't care
        if (EventMoveEnd(monster)) {} // don't care
        if (EventSkillFinished(monster)) {} // don't care
        if (EventTargetDisappeared(monster)) {} // don't care

        return "IDLE"; // nothing interesting happened
    }

    string UpdateServer_MOVING(Monster monster)
    {
        // events sorted by priority (e.g. target doesn't matter if we died)
        if (EventDied(monster))
        {
            // we died.
            monster.movement.Reset();
            return "DEAD";
        }
        if (EventStunned(monster))
        {
            monster.movement.Reset();
            return "STUNNED";
        }
        if (EventMoveEnd(monster))
        {
            // we reached our destination.
            return "IDLE";
        }
        if (EventTargetDied(monster))
        {
            // we had a target before, but it died now. clear it.
            monster.target = null;
            monster.skills.CancelCast();
            monster.movement.Reset();
            return "IDLE";
        }
        if (EventTargetTooFarToFollow(monster))
        {
            // we had a target before, but it's out of follow range now.
            // clear it and go back to start. don't stay here.
            monster.target = null;
            monster.skills.CancelCast();
            monster.movement.Navigate(monster.startPosition, 0);
            return "MOVING";
        }
        if (EventTargetTooFarToAttack(monster))
        {
            // we had a target before, but it's out of attack range now.
            // follow it. (use collider point(s) to also work with big entities)
            float stoppingDistance = ((MonsterSkills)monster.skills).CurrentCastRange() * attackToMoveRangeRatio;
            Vector3 destination = Utils.ClosestPoint(monster.target, monster.transform.position);
            monster.movement.Navigate(destination, stoppingDistance);
            return "MOVING";
        }
        if (EventTargetEnteredSafeZone(monster))
        {
            // if our target entered the safe zone, we need to be really careful
            // to avoid kiting.
            // -> players could pull a monster near a safe zone and then step in
            //    and out of it before/after attacks without ever getting hit by
            //    the monster
            // -> running back to start won't help, can still kit while running
            // -> warping back to start won't help, we might accidentally placed
            //    a monster in attack range of a safe zone
            // -> the 100% secure way is to die and hide it immediately. many
            //    popular MMOs do it the same way to avoid exploits.
            // => call Entity.OnDeath without rewards etc. and hide immediately
            monster.OnDeath(); // no looting
            monster.respawnTimeEnd = NetworkTime.time + monster.respawnTime; // respawn in a while
            return "DEAD";
        }
        if (EventAggro(monster))
        {
            // target in attack range. try to cast a first skill on it
            // (we may get a target while randomly wandering around)
            if (monster.skills.skills.Count > 0) monster.skills.currentSkill = ((MonsterSkills)monster.skills).NextSkill();
            else Debug.LogError(name + " has no skills to attack with.");
            monster.movement.Reset();
            return "IDLE";
        }
        if (EventDeathTimeElapsed(monster)) {} // don't care
        if (EventRespawnTimeElapsed(monster)) {} // don't care
        if (EventSkillFinished(monster)) {} // don't care
        if (EventTargetDisappeared(monster)) {} // don't care
        if (EventSkillRequest(monster)) {} // don't care, finish movement first
        if (EventMoveRandomly(monster)) {} // don't care

        return "MOVING"; // nothing interesting happened
    }

    string UpdateServer_CASTING(Monster monster)
    {
        // keep looking at the target for server & clients (only Y rotation)
        if (monster.target)
            monster.movement.LookAtY(monster.target.transform.position);

        // events sorted by priority (e.g. target doesn't matter if we died)
        if (EventDied(monster))
        {
            // we died.
            return "DEAD";
        }
        if (EventStunned(monster))
        {
            monster.skills.CancelCast();
            monster.movement.Reset();
            return "STUNNED";
        }
        if (EventTargetDisappeared(monster))
        {
            // cancel if the target matters for this skill
            if (monster.skills.skills[monster.skills.currentSkill].cancelCastIfTargetDied)
            {
                monster.skills.CancelCast();
                monster.target = null;
                return "IDLE";
            }
        }
        if (EventTargetDied(monster))
        {
            // cancel if the target matters for this skill
            if (monster.skills.skills[monster.skills.currentSkill].cancelCastIfTargetDied)
            {
                monster.skills.CancelCast();
                monster.target = null;
                return "IDLE";
            }
        }
        if (EventTargetEnteredSafeZone(monster))
        {
            // cancel if the target matters for this skill
            if (monster.skills.skills[monster.skills.currentSkill].cancelCastIfTargetDied)
            {
                // if our target entered the safe zone, we need to be really careful
                // to avoid kiting.
                // -> players could pull a monster near a safe zone and then step in
                //    and out of it before/after attacks without ever getting hit by
                //    the monster
                // -> running back to start won't help, can still kit while running
                // -> warping back to start won't help, we might accidentally placed
                //    a monster in attack range of a safe zone
                // -> the 100% secure way is to die and hide it immediately. many
                //    popular MMOs do it the same way to avoid exploits.
                // => call Entity.OnDeath without rewards etc. and hide immediately
                monster.OnDeath(); // no looting
                monster.respawnTimeEnd = NetworkTime.time + monster.respawnTime; // respawn in a while
                return "DEAD";
            }
        }
        if (EventSkillFinished(monster))
        {
            // finished casting. apply the skill on the target.
            monster.skills.FinishCast(monster.skills.skills[monster.skills.currentSkill]);

            // did the target die? then clear it so that the monster doesn't
            // run towards it if the target respawned
            // (target might be null if disappeared or targetless skill)
            if (monster.target != null && monster.target.health.current == 0)
                monster.target = null;

            // go back to IDLE, reset current skill
            ((MonsterSkills)monster.skills).lastSkill = monster.skills.currentSkill;
            monster.skills.currentSkill = -1;
            return "IDLE";
        }
        if (EventDeathTimeElapsed(monster)) {} // don't care
        if (EventRespawnTimeElapsed(monster)) {} // don't care
        if (EventMoveEnd(monster)) {} // don't care
        if (EventTargetTooFarToAttack(monster)) {} // don't care, we were close enough when starting to cast
        if (EventTargetTooFarToFollow(monster)) {} // don't care, we were close enough when starting to cast
        if (EventAggro(monster)) {} // don't care, always have aggro while casting
        if (EventSkillRequest(monster)) {} // don't care, that's why we are here
        if (EventMoveRandomly(monster)) {} // don't care

        return "CASTING"; // nothing interesting happened
    }

    string UpdateServer_STUNNED(Monster monster)
    {
        // events sorted by priority (e.g. target doesn't matter if we died)
        if (EventDied(monster))
        {
            // we died.
            return "DEAD";
        }
        if (EventStunned(monster))
        {
            return "STUNNED";
        }

        // go back to idle if we aren't stunned anymore and process all new
        // events there too
        return "IDLE";
    }

    string UpdateServer_DEAD(Monster monster)
    {
        // events sorted by priority (e.g. target doesn't matter if we died)
        if (EventRespawnTimeElapsed(monster))
        {
            // respawn at the start position with full health, visibility, no loot
            monster.gold = 0;
            monster.inventory.slots.Clear();
            monster.Show();
            monster.movement.Warp(monster.startPosition); // recommended over transform.position
            monster.Revive();
            return "IDLE";
        }
        if (EventDeathTimeElapsed(monster))
        {
            // we were lying around dead for long enough now.
            // hide while respawning, or disappear forever
            if (monster.respawn) monster.Hide();
            else NetworkServer.Destroy(monster.gameObject);
            return "DEAD";
        }
        if (EventSkillRequest(monster)) {} // don't care
        if (EventSkillFinished(monster)) {} // don't care
        if (EventMoveEnd(monster)) {} // don't care
        if (EventTargetDisappeared(monster)) {} // don't care
        if (EventTargetDied(monster)) {} // don't care
        if (EventTargetTooFarToFollow(monster)) {} // don't care
        if (EventTargetTooFarToAttack(monster)) {} // don't care
        if (EventTargetEnteredSafeZone(monster)) {} // don't care
        if (EventAggro(monster)) {} // don't care
        if (EventMoveRandomly(monster)) {} // don't care
        if (EventStunned(monster)) {} // don't care
        if (EventDied(monster)) {} // don't care, of course we are dead

        return "DEAD"; // nothing interesting happened
    }

    public override string UpdateServer(Entity entity)
    {
        Monster monster = (Monster)entity;

        if (monster.state == "IDLE")    return UpdateServer_IDLE(monster);
        if (monster.state == "MOVING")  return UpdateServer_MOVING(monster);
        if (monster.state == "CASTING") return UpdateServer_CASTING(monster);
        if (monster.state == "STUNNED") return UpdateServer_STUNNED(monster);
        if (monster.state == "DEAD")    return UpdateServer_DEAD(monster);

        Debug.LogError("invalid state:" + monster.state);
        return "IDLE";
    }

    public override void UpdateClient(Entity entity)
    {
        Monster monster = (Monster)entity;
        if (monster.state == "CASTING")
        {
            // keep looking at the target for server & clients (only Y rotation)
            if (monster.target)
                monster.movement.LookAtY(monster.target.transform.position);
        }
    }

    // DrawGizmos can be used for debug info
    public override void DrawGizmos(Entity entity)
    {
        Monster monster = (Monster)entity;

        // draw the movement area (around 'start' if game running,
        // or around current position if still editing)
        Vector3 startHelp = Application.isPlaying ? monster.startPosition : monster.transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(startHelp, moveDistance);

        // draw the follow dist
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(startHelp, followDistance);
    }
}
