// some common events shared amongst all our brain implementations.
// -> it's not necessary to inherit from CommonBrain for your custom brains.
//    this is just to save redundancies.
using UnityEngine;
using Mirror;

public abstract class CommonBrain : ScriptableBrain
{
    public bool EventAggro(Entity entity) =>
        entity.target != null && entity.target.health.current > 0;

    public bool EventDied(Entity entity) =>
        entity.health.current == 0;

    // only fire when stopped moving
    public bool EventMoveEnd(Entity entity) =>
        entity.state == "MOVING" && !entity.movement.IsMoving();

    // only fire when started moving
    public bool EventMoveStart(Entity entity) =>
        entity.state != "MOVING" && entity.movement.IsMoving();

    public bool EventSkillFinished(Entity entity) =>
        0 <= entity.skills.currentSkill && entity.skills.currentSkill < entity.skills.skills.Count &&
        entity.skills.skills[entity.skills.currentSkill].CastTimeRemaining() == 0;

    public bool EventSkillRequest(Entity entity) =>
        0 <= entity.skills.currentSkill && entity.skills.currentSkill < entity.skills.skills.Count;

    public bool EventStunned(Entity entity) =>
        NetworkTime.time <= entity.stunTimeEnd;

    public bool EventTargetDied(Entity entity) =>
        entity.target != null && entity.target.health.current == 0;

    public bool EventTargetDisappeared(Entity entity) =>
        entity.target == null;

    public bool EventTargetEnteredSafeZone(Entity entity) =>
        entity.target != null && entity.target.inSafeZone;

    public bool EventTargetTooFarToAttack(Entity entity) =>
        entity.target != null &&
        0 <= entity.skills.currentSkill && entity.skills.currentSkill < entity.skills.skills.Count &&
        !entity.skills.CastCheckDistance(entity.skills.skills[entity.skills.currentSkill], out Vector3 destination);
}
