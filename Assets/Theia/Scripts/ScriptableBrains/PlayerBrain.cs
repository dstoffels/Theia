// npc brain does nothing but stand around
using UnityEngine;
using Mirror;

[CreateAssetMenu(menuName="uMMORPG Brain/Brains/Player", order=999)]
public class PlayerBrain : CommonBrain
{
    [Tooltip("Being stunned interrupts the cast. Enable this option to continue the cast afterwards.")]
    public bool continueCastAfterStunned = true;

    // events //////////////////////////////////////////////////////////////////
    public bool EventCancelAction(Player player)
    {
        bool result = player.cancelActionRequested;
        player.cancelActionRequested = false; // reset
        return result;
    }

    public bool EventCraftingStarted(Player player)
    {
        bool result = player.crafting.requestPending;
        player.crafting.requestPending = false;
        return result;
    }

    public bool EventCraftingDone(Player player) =>
        player.state == "CRAFTING" && NetworkTime.time > player.crafting.endTime;

    public bool EventRespawn(Player player)
    {
        bool result = player.respawnRequested;
        player.respawnRequested = false; // reset
        return result;
    }

    // trade canceled or finished?
    public bool EventTradeDone(Player player) =>
        player.state == "TRADING" && player.trading.requestFrom == "";

    // did someone request a trade? and did we request a trade with him too?
    public bool EventTradeStarted(Player player)
    {
        Player other = player.trading.FindPlayerFromInvitation();
        return other != null && other.trading.requestFrom == player.name;
    }

    // states //////////////////////////////////////////////////////////////////
    string UpdateServer_IDLE(Player player)
    {
        // events sorted by priority (e.g. target doesn't matter if we died)
        if (EventDied(player))
        {
            // we died.
            return "DEAD";
        }
        if (EventStunned(player))
        {
            player.movement.Reset();
            return "STUNNED";
        }
        if (EventCancelAction(player))
        {
            // the only thing that we can cancel is the target
            player.target = null;
            return "IDLE";
        }
        if (EventTradeStarted(player))
        {
            // cancel casting (if any), set target, go to trading
            player.skills.CancelCast(); // just in case
            player.target = player.trading.FindPlayerFromInvitation();
            return "TRADING";
        }
        if (EventCraftingStarted(player))
        {
            // cancel casting (if any), go to crafting
            player.skills.CancelCast(); // just in case
            return "CRAFTING";
        }
        if (EventMoveStart(player))
        {
            // cancel casting (if any)
            player.skills.CancelCast();
            return "MOVING";
        }
        if (EventSkillRequest(player))
        {
            // don't cast while mounted
            // (no MOUNTED state because we'd need MOUNTED_STUNNED, etc. too)
            if (!player.mountControl.IsMounted())
            {
                // user wants to cast a skill.
                // check self (alive, mana, weapon etc.) and target and distance
                Skill skill = player.skills.skills[player.skills.currentSkill];
                player.nextTarget = player.target; // return to this one after any corrections by skills.CastCheckTarget
                if (player.skills.CastCheckSelf(skill) &&
                    player.skills.CastCheckTarget(skill) &&
                    player.skills.CastCheckDistance(skill, out Vector3 destination))
                {
                    // start casting and cancel movement in any case
                    // (player might move into attack range * 0.8 but as soon as we
                    //  are close enough to cast, we fully commit to the cast.)
                    player.movement.Reset();
                    player.skills.StartCast(skill);
                    return "CASTING";
                }
                else
                {
                    // checks failed. reset the attempted current skill
                    player.skills.currentSkill = -1;
                    player.nextTarget = null; // nevermind, clear again (otherwise it's shown in UITarget)
                    return "IDLE";
                }
            }
        }
        if (EventSkillFinished(player)) {} // don't care
        if (EventMoveEnd(player)) {} // don't care
        if (EventTradeDone(player)) {} // don't care
        if (EventCraftingDone(player)) {} // don't care
        if (EventRespawn(player)) {} // don't care
        if (EventTargetDied(player)) {} // don't care
        if (EventTargetDisappeared(player)) {} // don't care

        return "IDLE"; // nothing interesting happened
    }

    string UpdateServer_MOVING(Player player)
    {
        // events sorted by priority (e.g. target doesn't matter if we died)
        if (EventDied(player))
        {
            // we died.
            return "DEAD";
        }
        if (EventStunned(player))
        {
            player.movement.Reset();
            return "STUNNED";
        }
        if (EventMoveEnd(player))
        {
            // finished moving. do whatever we did before.
            return "IDLE";
        }
        if (EventCancelAction(player))
        {
            // cancel casting (if any) and stop moving
            player.skills.CancelCast();
            //player.movement.Reset(); <- done locally. doing it here would reset localplayer to the slightly behind server position otherwise
            return "IDLE";
        }
        if (EventTradeStarted(player))
        {
            // cancel casting (if any), stop moving, set target, go to trading
            player.skills.CancelCast();
            player.movement.Reset();
            player.target = player.trading.FindPlayerFromInvitation();
            return "TRADING";
        }
        if (EventCraftingStarted(player))
        {
            // cancel casting (if any), stop moving, go to crafting
            player.skills.CancelCast();
            player.movement.Reset();
            return "CRAFTING";
        }
        // SPECIAL CASE: Skill Request while doing rubberband movement
        // -> we don't really need to react to it
        // -> we could just wait for move to end, then react to request in IDLE
        // -> BUT player position on server always lags behind in rubberband movement
        // -> SO there would be a noticeable delay before we start to cast
        //
        // SOLUTION:
        // -> start casting as soon as we are in range
        // -> BUT don't ResetMovement. instead let it slide to the final position
        //    while already starting to cast
        // -> NavMeshAgentRubberbanding won't accept new positions while casting
        //    anyway, so this is fine
        if (EventSkillRequest(player))
        {
            // don't cast while mounted
            // (no MOUNTED state because we'd need MOUNTED_STUNNED, etc. too)
            if (!player.mountControl.IsMounted())
            {
                Skill skill = player.skills.skills[player.skills.currentSkill];
                if (player.skills.CastCheckSelf(skill) &&
                    player.skills.CastCheckTarget(skill) &&
                    player.skills.CastCheckDistance(skill, out Vector3 destination))
                {
                    //Debug.Log("MOVING->EventSkillRequest: early cast started while sliding to destination...");
                    // player.rubberbanding.ResetMovement(); <- DO NOT DO THIS.
                    player.skills.StartCast(skill);
                    return "CASTING";
                }
            }
        }
        if (EventMoveStart(player)) {} // don't care
        if (EventSkillFinished(player)) {} // don't care
        if (EventTradeDone(player)) {} // don't care
        if (EventCraftingDone(player)) {} // don't care
        if (EventRespawn(player)) {} // don't care
        if (EventTargetDied(player)) {} // don't care
        if (EventTargetDisappeared(player)) {} // don't care

        return "MOVING"; // nothing interesting happened
    }

    void UseNextTargetIfAny(Player player)
    {
        // use next target if the user tried to target another while casting
        // (target is locked while casting so skill isn't applied to an invalid
        //  target accidentally)
        if (player.nextTarget != null)
        {
            player.target = player.nextTarget;
            player.nextTarget = null;
        }
    }

    string UpdateServer_CASTING(Player player)
    {
        // keep looking at the target for server & clients (only Y rotation)
        if (player.target && player.movement.DoCombatLookAt())
            player.movement.LookAtY(player.target.transform.position);

        // events sorted by priority (e.g. target doesn't matter if we died)
        //
        // IMPORTANT: nextTarget might have been set while casting, so make sure
        // to handle it in any case here. it should definitely be null again
        // after casting was finished.
        // => this way we can reliably display nextTarget on the client if it's
        //    != null, so that UITarget always shows nextTarget>target
        //    (this just feels better)
        if (EventDied(player))
        {
            // we died.
            UseNextTargetIfAny(player); // if user selected a new target while casting
            return "DEAD";
        }
        if (EventStunned(player))
        {
            // cancel cast & movement
            // (only clear current skill if we don't continue cast after stunned)
            player.skills.CancelCast(!continueCastAfterStunned);
            player.movement.Reset();
            return "STUNNED";
        }
        if (EventMoveStart(player))
        {
            // we do NOT cancel the cast if the player moved, and here is why:
            // * local player might move into cast range and then try to cast.
            // * server then receives the Cmd, goes to CASTING state, then
            //   receives one of the last movement updates from the local player
            //   which would cause EventMoveStart and cancel the cast.
            // * this is the price for rubberband movement.
            // => if the player wants to cast and got close enough, then we have
            //    to fully commit to it. there is no more way out except via
            //    cancel action. any movement in here is to be rejected.
            //    (many popular MMOs have the same behaviour too)
            //

            // we do NOT reset movement either. allow sliding to final position.
            // (NavMeshAgentRubberbanding doesn't accept new ones while CASTING)
            //player.movement.Reset(); <- DO NOT DO THIS

            // we do NOT return "CASTING". EventMoveStart would constantly fire
            // while moving for skills that allow movement. hence we would
            // always return "CASTING" here and never get to the castfinished
            // code below.
            //return "CASTING";
        }
        if (EventCancelAction(player))
        {
            // cancel casting
            player.skills.CancelCast();
            UseNextTargetIfAny(player); // if user selected a new target while casting
            return "IDLE";
        }
        if (EventTradeStarted(player))
        {
            // cancel casting (if any), stop moving, set target, go to trading
            player.skills.CancelCast();
            player.movement.Reset();

            // set target to trade target instead of next target (clear that)
            player.target = player.trading.FindPlayerFromInvitation();
            player.nextTarget = null;
            return "TRADING";
        }
        if (EventTargetDisappeared(player))
        {
            // cancel if the target matters for this skill
            if (player.skills.skills[player.skills.currentSkill].cancelCastIfTargetDied)
            {
                player.skills.CancelCast();
                UseNextTargetIfAny(player); // if user selected a new target while casting
                return "IDLE";
            }
        }
        if (EventTargetDied(player))
        {
            // cancel if the target matters for this skill
            if (player.skills.skills[player.skills.currentSkill].cancelCastIfTargetDied)
            {
                player.skills.CancelCast();
                UseNextTargetIfAny(player); // if user selected a new target while casting
                return "IDLE";
            }
        }
        if (EventSkillFinished(player))
        {
            // apply the skill after casting is finished
            // note: we don't check the distance again. it's more fun if players
            //       still cast the skill if the target ran a few steps away
            Skill skill = player.skills.skills[player.skills.currentSkill];

            // apply the skill on the target
            player.skills.FinishCast(skill);

            // clear current skill for now
            player.skills.currentSkill = -1;

            // use next target if the user tried to target another while casting
            UseNextTargetIfAny(player);

            // go back to IDLE
            return "IDLE";
        }
        if (EventMoveEnd(player)) {} // don't care
        if (EventTradeDone(player)) {} // don't care
        if (EventCraftingStarted(player)) {} // don't care
        if (EventCraftingDone(player)) {} // don't care
        if (EventRespawn(player)) {} // don't care
        if (EventSkillRequest(player)) {} // don't care

        return "CASTING"; // nothing interesting happened
    }

    string UpdateServer_STUNNED(Player player)
    {
        // events sorted by priority (e.g. target doesn't matter if we died)
        if (EventDied(player))
        {
            // we died.
            return "DEAD";
        }
        if (EventStunned(player))
        {
            return "STUNNED";
        }

        // go back to idle if we aren't stunned anymore and process all new
        // events there too
        return "IDLE";
    }

    string UpdateServer_TRADING(Player player)
    {
        // events sorted by priority (e.g. target doesn't matter if we died)
        if (EventDied(player))
        {
            // we died, stop trading. other guy will receive targetdied event.
            player.trading.Cleanup();
            return "DEAD";
        }
        if (EventStunned(player))
        {
            // stop trading
            player.skills.CancelCast();
            player.movement.Reset();
            player.trading.Cleanup();
            return "STUNNED";
        }
        if (EventMoveStart(player))
        {
            // reject movement while trading
            player.movement.Reset();
            return "TRADING";
        }
        if (EventCancelAction(player))
        {
            // stop trading
            player.trading.Cleanup();
            return "IDLE";
        }
        if (EventTargetDisappeared(player))
        {
            // target disconnected, stop trading
            player.trading.Cleanup();
            return "IDLE";
        }
        if (EventTargetDied(player))
        {
            // target died, stop trading
            player.trading.Cleanup();
            return "IDLE";
        }
        if (EventTradeDone(player))
        {
            // someone canceled or we finished the trade. stop trading
            player.trading.Cleanup();
            return "IDLE";
        }
        if (EventMoveEnd(player)) {} // don't care
        if (EventSkillFinished(player)) {} // don't care
        if (EventCraftingStarted(player)) {} // don't care
        if (EventCraftingDone(player)) {} // don't care
        if (EventRespawn(player)) {} // don't care
        if (EventTradeStarted(player)) {} // don't care
        if (EventSkillRequest(player)) {} // don't care

        return "TRADING"; // nothing interesting happened
    }

    string UpdateServer_CRAFTING(Player player)
    {
        // events sorted by priority (e.g. target doesn't matter if we died)
        if (EventDied(player))
        {
            // we died, stop crafting
            return "DEAD";
        }
        if (EventStunned(player))
        {
            // stop crafting
            player.movement.Reset();
            return "STUNNED";
        }
        if (EventMoveStart(player))
        {
            // reject movement while crafting
            player.movement.Reset();
            return "CRAFTING";
        }
        if (EventCraftingDone(player))
        {
            // finish crafting
            player.crafting.Craft();
            return "IDLE";
        }
        if (EventCancelAction(player)) {} // don't care. user pressed craft, we craft.
        if (EventTargetDisappeared(player)) {} // don't care
        if (EventTargetDied(player)) {} // don't care
        if (EventMoveEnd(player)) {} // don't care
        if (EventSkillFinished(player)) {} // don't care
        if (EventRespawn(player)) {} // don't care
        if (EventTradeStarted(player)) {} // don't care
        if (EventTradeDone(player)) {} // don't care
        if (EventCraftingStarted(player)) {} // don't care
        if (EventSkillRequest(player)) {} // don't care

        return "CRAFTING"; // nothing interesting happened
    }

    string UpdateServer_DEAD(Player player)
    {
        // events sorted by priority (e.g. target doesn't matter if we died)
        if (EventRespawn(player))
        {
            // revive to closest spawn, with 50% health, then go to idle
            Transform start = NetworkManagerMMO.GetNearestStartPosition(player.transform.position);
            player.movement.Warp(start.position); // recommended over transform.position
            player.Revive(0.5f);
            return "IDLE";
        }
        if (EventMoveStart(player))
        {
            // if a player gets killed while sliding down a slope or while in
            // the air then he might continue to move after dead. it's fine as
            // long as we don't allow client input to control the move.
            return "DEAD";
        }
        if (EventMoveEnd(player)) {} // don't care
        if (EventSkillFinished(player)) {} // don't care
        if (EventDied(player)) {} // don't care
        if (EventCancelAction(player)) {} // don't care
        if (EventTradeStarted(player)) {} // don't care
        if (EventTradeDone(player)) {} // don't care
        if (EventCraftingStarted(player)) {} // don't care
        if (EventCraftingDone(player)) {} // don't care
        if (EventTargetDisappeared(player)) {} // don't care
        if (EventTargetDied(player)) {} // don't care
        if (EventSkillRequest(player)) {} // don't care

        return "DEAD"; // nothing interesting happened
    }

    public override string UpdateServer(Entity entity)
    {
        Player player = (Player)entity;

        if (player.state == "IDLE")     return UpdateServer_IDLE(player);
        if (player.state == "MOVING")   return UpdateServer_MOVING(player);
        if (player.state == "CASTING")  return UpdateServer_CASTING(player);
        if (player.state == "STUNNED")  return UpdateServer_STUNNED(player);
        if (player.state == "TRADING")  return UpdateServer_TRADING(player);
        if (player.state == "CRAFTING") return UpdateServer_CRAFTING(player);
        if (player.state == "DEAD")     return UpdateServer_DEAD(player);

        Debug.LogError("invalid state:" + player.state);
        return "IDLE";
    }
    public override void UpdateClient(Entity entity)
    {
        Player player = (Player)entity;

        if (player.state == "IDLE" || player.state == "MOVING")
        {
            if (player.isLocalPlayer)
            {
                // cancel action if escape key was pressed
                if (Input.GetKeyDown(player.cancelActionKey))
                {
                    player.movement.Reset(); // reset locally because we use rubberband movement
                    player.CmdCancelAction();
                }

                // trying to cast a skill on a monster that wasn't in range?
                // then check if we walked into attack range by now
                if (player.useSkillWhenCloser != -1)
                {
                    // can we still attack the target? maybe it was switched.
                    if (player.CanAttack(player.target))
                    {
                        // in range already?
                        // -> we don't use skills.CastCheckDistance because we want to
                        // move a bit closer (attackToMoveRangeRatio)
                        float range = player.skills.skills[player.useSkillWhenCloser].castRange * player.attackToMoveRangeRatio;
                        if (Utils.ClosestDistance(player, player.target) <= range)
                        {
                            // then stop moving and start attacking
                            ((PlayerSkills)player.skills).CmdUse(player.useSkillWhenCloser);

                            // reset
                            player.useSkillWhenCloser = -1;
                        }
                        // otherwise keep walking there. the target might move
                        // around or run away, so we need to keep adjusting the
                        // destination all the time
                        else
                        {
                            //Debug.Log("walking closer to target...");
                            Vector3 destination = Utils.ClosestPoint(player.target, player.transform.position);
                            player.movement.Navigate(destination, range);
                        }
                    }
                    // otherwise reset
                    else player.useSkillWhenCloser = -1;
                }
            }
        }
        else if (player.state == "CASTING")
        {
            // keep looking at the target for server & clients (only Y rotation)
            if (player.target && player.movement.DoCombatLookAt())
                player.movement.LookAtY(player.target.transform.position);

            if (player.isLocalPlayer)
            {
                // reset any client sided movement
                player.movement.Reset();

                // cancel action if escape key was pressed
                if (Input.GetKeyDown(player.cancelActionKey)) player.CmdCancelAction();
            }
        }
        else if (player.state == "STUNNED")
        {
            if (player.isLocalPlayer)
            {
                // reset any client sided movement
                player.movement.Reset();

                // cancel action if escape key was pressed
                if (Input.GetKeyDown(player.cancelActionKey)) player.CmdCancelAction();
            }
        }
        else if (player.state == "TRADING") {}
        else if (player.state == "CRAFTING") {}
        else if (player.state == "DEAD") {}
        else Debug.LogError("invalid state:" + player.state);
    }
}
