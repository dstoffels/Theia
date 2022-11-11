// some common events shared amongst all our brain implementations.
// -> it's not necessary to inherit from CommonBrain for your custom brains.
//    this is just to save redundancies.
using UnityEngine;
using Mirror;

public abstract class CommonBrain : ScriptableBrain
{
    public bool EventAggro(EntityOLD entity) =>
        entity.target != null && entity.target.health.current > 0;

    public bool EventDied(EntityOLD entity) =>
        entity.health.current == 0;

    // only fire when stopped moving
    public bool EventMoveEnd(EntityOLD entity) =>
        entity.state == "MOVING" && !entity.movement.IsMoving();

    // only fire when started moving
    public bool EventMoveStart(EntityOLD entity) =>
        entity.state != "MOVING" && entity.movement.IsMoving();

    public bool EventSkillFinished(EntityOLD entity) =>
        0 <= entity.skillsOLD.currentSkill && entity.skillsOLD.currentSkill < entity.skillsOLD.skills.Count &&
        entity.skillsOLD.skills[entity.skillsOLD.currentSkill].CastTimeRemaining() == 0;

    public bool EventSkillRequest(EntityOLD entity) =>
        0 <= entity.skillsOLD.currentSkill && entity.skillsOLD.currentSkill < entity.skillsOLD.skills.Count;

    public bool EventStunned(EntityOLD entity) =>
        NetworkTime.time <= entity.stunTimeEnd;

    public bool EventTargetDied(EntityOLD entity) =>
        entity.target != null && entity.target.health.current == 0;

    public bool EventTargetDisappeared(EntityOLD entity) =>
        entity.target == null;

    public bool EventTargetEnteredSafeZone(EntityOLD entity) =>
        entity.target != null && entity.target.inSafeZone;

    public bool EventTargetTooFarToAttack(EntityOLD entity) =>
        entity.target != null &&
        0 <= entity.skillsOLD.currentSkill && entity.skillsOLD.currentSkill < entity.skillsOLD.skills.Count &&
        !entity.skillsOLD.CastCheckDistance(entity.skillsOLD.skills[entity.skillsOLD.currentSkill], out Vector3 destination);
}
