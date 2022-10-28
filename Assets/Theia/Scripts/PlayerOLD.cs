// All player logic was put into this class. We could also split it into several
// smaller components, but this would result in many GetComponent calls and a
// more complex syntax.
//
// The default Player class takes care of the basic player logic like the state
// machine and some properties like damage and defense.
//
// The Player class stores the maximum experience for each level in a simple
// array. So the maximum experience for level 1 can be found in expMax[0] and
// the maximum experience for level 2 can be found in expMax[1] and so on. The
// player's health and mana are also level dependent in most MMORPGs, hence why
// there are hpMax and mpMax arrays too. We can find out a players's max health
// in level 1 by using hpMax[0] and so on.
//
// The class also takes care of selection handling, which detects 3D world
// clicks and then targets/navigates somewhere/interacts with someone.
//
// Animations are not handled by the NetworkAnimator because it's still very
// buggy and because it can't really react to movement stops fast enough, which
// results in moonwalking. Not synchronizing animations over the network will
// also save us bandwidth
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Mirror;
using TMPro;

[Serializable] public class UnityEventPlayer : UnityEvent<PlayerOLD> {}

[RequireComponent(typeof(Experience))]
[RequireComponent(typeof(Intelligence))]
[RequireComponent(typeof(Strength))]
[RequireComponent(typeof(PlayerChat))]
[RequireComponent(typeof(PlayerCrafting))]
[RequireComponent(typeof(PlayerGameMasterTool))]
[RequireComponent(typeof(PlayerGuild))]
[RequireComponent(typeof(PlayerIndicator))]
[RequireComponent(typeof(PlayerInventory))]
[RequireComponent(typeof(PlayerItemMall))]
[RequireComponent(typeof(PlayerLooting))]
[RequireComponent(typeof(PlayerMountControl))]
[RequireComponent(typeof(PlayerNpcRevive))]
[RequireComponent(typeof(PlayerNpcTeleport))]
[RequireComponent(typeof(PlayerNpcTrading))]
[RequireComponent(typeof(PlayerParty))]
[RequireComponent(typeof(PlayerPetControl))]
[RequireComponent(typeof(PlayerQuests))]
[RequireComponent(typeof(PlayerSkillbar))]
[RequireComponent(typeof(PlayerSkills))]
[RequireComponent(typeof(PlayerTrading))]
[RequireComponent(typeof(NetworkName))]
public partial class PlayerOLD : EntityOLD
{
    // fields for all player components to avoid costly GetComponent calls
    [Header("Components")]
    public Experience experience;
    public Intelligence intelligence;
    public Strength strength;
    public PlayerChat chat;
    public PlayerCrafting crafting;
    public PlayerGameMasterTool gameMasterTool;
    public PlayerGuild guild;
    public PlayerIndicator indicator;
    public PlayerInventory inventory;
    public PlayerItemMall itemMall;
    public PlayerLooting looting;
    public PlayerMountControl mountControl;
    public PlayerNpcRevive npcRevive;
    public PlayerNpcTeleport npcTeleport;
    public PlayerNpcTrading npcTrading;
    public PlayerParty party;
    public PlayerPetControl petControl;
    public PlayerQuests quests;
    public PlayerSkillbar skillbar;
    public PlayerTrading trading;
    public Camera avatarCamera;

    [Header("Text Meshes")]
    public TextMeshPro nameOverlay;
    public Color nameOverlayDefaultColor = Color.white;
    public Color nameOverlayOffenderColor = Color.magenta;
    public Color nameOverlayMurdererColor = Color.red;
    public Color nameOverlayPartyColor = new Color(0.341f, 0.965f, 0.702f);
    public string nameOverlayGameMasterPrefix = "[GM] ";

    [Header("Icons")]
    public Sprite classIcon; // for character selection
    public Sprite portraitIcon; // for top left portrait

    // some meta info
    [HideInInspector] public string account = "";
    [HideInInspector] public string className = "";

    // keep the GM flag in here and the controls in PlayerGameMaster.cs:
    // -> we need the flag for NameOverlay prefix anyway
    // -> it might be needed outside of PlayerGameMaster for other GM specific
    //    mechanics/checks later
    // -> this way we can use SyncToObservers for the flag, and SyncToOwner for
    //    everything else in PlayerGameMaster component. this is a LOT easier.
    [SyncVar] public bool isGameMaster;

    // localPlayer singleton for easier access from UI scripts etc.
    public static PlayerOLD localPlayer;

    // speed
    public override float speed
    {
        get
        {
            // mount speed if mounted, regular speed otherwise
            return mountControl.activeMount != null && mountControl.activeMount.health.current > 0
                   ? mountControl.activeMount.speed
                   : base.speed;
        }
    }

    // item cooldowns
    // it's based on a 'cooldownCategory' that can be set in ScriptableItems.
    // -> they can use their own name for a cooldown that only applies to them
    // -> they can use a category like 'HealthPotion' for a shared cooldown
    //    amongst all health potions
    // => we could use hash(category) as key to significantly reduce bandwidth,
    //    but we don't anymore because it makes database saving easier.
    //    otherwise we would have to find the category from a hash.
    // => IMPORTANT: cooldowns need to be saved in database so that long
    //    cooldowns can't be circumvented by logging out and back in again.
    internal SyncDictionary<string, double> itemCooldowns = new SyncDictionary<string, double>();

    [Header("Interaction")]
    public float interactionRange = 4;
    public bool localPlayerClickThrough = true; // click selection goes through localplayer. feels best.
    public KeyCode cancelActionKey = KeyCode.Escape;

    [Header("PvP")]
    public BuffSkill offenderBuff;
    public BuffSkill murdererBuff;

    // when moving into attack range of a target, we always want to move a
    // little bit closer than necessary to tolerate for latency and other
    // situations where the target might have moved away a little bit already.
    [Header("Movement")]
    [Range(0.1f, 1)] public float attackToMoveRangeRatio = 0.8f;

    // some commands should have delays to avoid DDOS, too much database usage
    // or brute forcing coupons etc. we use one riskyAction timer for all.
    [SyncVar, HideInInspector] public double nextRiskyActionTime = 0; // double for long term precision

    // the next target to be set if we try to set it while casting
    [SyncVar, HideInInspector] public EntityOLD nextTarget;

    // cache players to save lots of computations
    // (otherwise we'd have to iterate NetworkServer.objects all the time)
    // => on server: all online players
    // => on client: all observed players
    public static Dictionary<string, PlayerOLD> onlinePlayers = new Dictionary<string, PlayerOLD>();

    // first allowed logout time after combat
    public double allowedLogoutTime => lastCombatTime + ((NetworkManagerMMO)NetworkManager.singleton).combatLogoutDelay;
    public double remainingLogoutTime => NetworkTime.time < allowedLogoutTime ? (allowedLogoutTime - NetworkTime.time) : 0;

    // helper variable to remember which skill to use when we walked close enough
    [HideInInspector] public int useSkillWhenCloser = -1;

    // networkbehaviour ////////////////////////////////////////////////////////
    public override void OnStartLocalPlayer()
    {
        // set singleton
        localPlayer = this;

        // setup camera targets
        GameObject.FindWithTag("MinimapCamera").GetComponent<CopyPosition>().target = transform;
        if (avatarCamera) avatarCamera.enabled = true; // avatar camera for local player
    }

    protected override void Start()
    {
        // do nothing if not spawned (=for character selection previews)
        if (!isServer && !isClient) return;

        base.Start();
        onlinePlayers[name] = this;
    }

    void LateUpdate()
    {
        // pass parameters to animation state machine
        // => passing the states directly is the most reliable way to avoid all
        //    kinds of glitches like movement sliding, attack twitching, etc.
        // => make sure to import all looping animations like idle/run/attack
        //    with 'loop time' enabled, otherwise the client might only play it
        //    once
        // => MOVING state is set to local IsMovement result directly. otherwise
        //    we would see animation latencies for rubberband movement if we
        //    have to wait for MOVING state to be received from the server
        // => MOVING checks if !CASTING because there is a case in UpdateMOVING
        //    -> SkillRequest where we still slide to the final position (which
        //    is good), but we should show the casting animation then.
        // => skill names are assumed to be boolean parameters in animator
        //    so we don't need to worry about an animation number etc.
        if (isClient) // no need for animations on the server
        {
            // now pass parameters after any possible rebinds
            foreach (Animator anim in GetComponentsInChildren<Animator>())
            {
                anim.SetBool("MOVING", movement.IsMoving() && !mountControl.IsMounted());
                anim.SetBool("CASTING", state == "CASTING");
                anim.SetBool("STUNNED", state == "STUNNED");
                anim.SetBool("MOUNTED", mountControl.IsMounted()); // for seated animation
                anim.SetBool("DEAD", state == "DEAD");
                foreach (SkillOLD skill in skills.skills)
                    if (skill.level > 0 && !(skill.data is PassiveSkill))
                        anim.SetBool(skill.name, skill.CastTimeRemaining() > 0);
            }
        }

        // update overlays in any case, except on server-only mode
        // (also update for character selection previews etc. then)
        if (!isServerOnly)
        {
            if (nameOverlay != null)
            {
                // only players need to copy names to name overlay. it never changes
                // for monsters / npcs.
                string prefix = isGameMaster ? nameOverlayGameMasterPrefix : "";
                nameOverlay.text = prefix + name;

                // find local player (null while in character selection)
                if (localPlayer != null)
                {
                    // note: murderer has higher priority (a player can be a murderer and an
                    // offender at the same time)
                    if (IsMurderer())
                        nameOverlay.color = nameOverlayMurdererColor;
                    else if (IsOffender())
                        nameOverlay.color = nameOverlayOffenderColor;
                    // member of the same party
                    else if (localPlayer.party.InParty() &&
                             localPlayer.party.party.Contains(name))
                        nameOverlay.color = nameOverlayPartyColor;
                    // otherwise default
                    else
                        nameOverlay.color = nameOverlayDefaultColor;
                }
            }
        }
    }

    void OnDestroy()
    {
        // try to remove from onlinePlayers first, NO MATTER WHAT
        // -> we can not risk ever not removing it. do this before any early
        //    returns etc.
        // -> ONLY remove if THIS object was saved. this avoids a bug where
        //    a host selects a character preview, then joins the game, then
        //    only after the end of the frame the preview is destroyed,
        //    OnDestroy is called and the preview would actually remove the
        //    world player from onlinePlayers. hence making guild management etc
        //    impossible.
        if (onlinePlayers.TryGetValue(name, out PlayerOLD entry) && entry == this)
            onlinePlayers.Remove(name);

        // do nothing if not spawned (=for character selection previews)
        if (!isServer && !isClient) return;

        if (isLocalPlayer)
            localPlayer = null;
    }

    // some brain events require Cmds that can't be in ScriptableObject ////////
    [Command]
    public void CmdRespawn() { respawnRequested = true; }
    internal bool respawnRequested;

    [Command]
    public void CmdCancelAction() { cancelActionRequested = true; }
    internal bool cancelActionRequested;

    // skill finished event & pending actions //////////////////////////////////
    // pending actions while casting. to be applied after cast.
    [HideInInspector] public int pendingSkill = -1;
    [HideInInspector] public Vector3 pendingDestination;
    [HideInInspector] public bool pendingDestinationValid;

    // client event when skill cast finished on server
    // -> useful for follow up attacks etc.
    //    (doing those on server won't really work because the target might have
    //     moved, in which case we need to follow, which we need to do on the
    //     client)
    [Client]
    public void OnSkillCastFinished(SkillOLD skill)
    {
        if (!isLocalPlayer) return;

        // tried to click move somewhere?
        if (pendingDestinationValid)
        {
            movement.Navigate(pendingDestination, 0);
        }
        // user pressed another skill button?
        else if (pendingSkill != -1)
        {
            ((PlayerSkills)skills).TryUse(pendingSkill, true);
        }
        // otherwise do follow up attack if no interruptions happened
        else if (skill.followupDefaultAttack)
        {
            ((PlayerSkills)skills).TryUse(0, true);
        }

        // clear pending actions in any case
        pendingSkill = -1;
        pendingDestinationValid = false;
    }

    // combat //////////////////////////////////////////////////////////////////
    [Server]
    public void OnDamageDealtTo(EntityOLD victim)
    {
        // attacked an innocent player
        if (victim is PlayerOLD && ((PlayerOLD)victim).IsInnocent())
        {
            // start offender if not a murderer yet
            if (!IsMurderer()) StartOffender();
        }
        // attacked a pet with an innocent owner
        else if (victim is Pet && ((Pet)victim).owner.IsInnocent())
        {
            // start offender if not a murderer yet
            if (!IsMurderer()) StartOffender();
        }
    }

    [Server]
    public void OnKilledEnemy(EntityOLD victim)
    {
        // killed an innocent player
        if (victim is PlayerOLD && ((PlayerOLD)victim).IsInnocent())
        {
            StartMurderer();
        }
        // killed a pet with an innocent owner
        else if (victim is Pet && ((Pet)victim).owner.IsInnocent())
        {
            StartMurderer();
        }
    }

    // aggro ///////////////////////////////////////////////////////////////////
    // this function is called by entities that attack us
    [ServerCallback]
    public override void OnAggro(EntityOLD entity)
    {
        // call base function
        base.OnAggro(entity);

        // forward to pet if it's supposed to defend us
        if (petControl.activePet != null && petControl.activePet.defendOwner)
            petControl.activePet.OnAggro(entity);
    }

    // movement ////////////////////////////////////////////////////////////////
    // check if movement is currently allowed
    // -> not in Movement.cs because we would have to add it to each player
    //    movement system. (can't use an abstract PlayerMovement.cs because
    //    PlayerNavMeshMovement needs to inherit from NavMeshMovement already)
    public bool IsMovementAllowed()
    {
        // some skills allow movement while casting
        bool castingAndAllowed = state == "CASTING" &&
                                 skills.currentSkill != -1 &&
                                 skills.skills[skills.currentSkill].allowMovement;

        // in a state where movement is allowed?
        // and if local player: not typing in an input?
        // (fix: only check for local player. checking in all cases means that
        //       no player could move if host types anything in an input)
        bool isLocalPlayerTyping = isLocalPlayer && UIUtils.AnyInputActive();
        return (state == "IDLE" || state == "MOVING" || castingAndAllowed) &&
               !isLocalPlayerTyping;
    }

    // death ///////////////////////////////////////////////////////////////////
    [Server]
    public override void OnDeath()
    {
        // take care of entity stuff
        base.OnDeath();

        // reset movement and navigation
        movement.Reset();
    }

    // item cooldowns //////////////////////////////////////////////////////////
    // get remaining item cooldown, or 0 if none
    public float GetItemCooldown(string cooldownCategory)
    {
        // find cooldown for that category
        if (itemCooldowns.TryGetValue(cooldownCategory, out double cooldownEnd))
        {
            return NetworkTime.time >= cooldownEnd ? 0 : (float)(cooldownEnd - NetworkTime.time);
        }

        // none found
        return 0;
    }

    // reset item cooldown
    public void SetItemCooldown(string cooldownCategory, float cooldown)
    {
        // save end time
        itemCooldowns[cooldownCategory] = NetworkTime.time + cooldown;
    }

    // attack //////////////////////////////////////////////////////////////////
    // CanAttack check
    // we use 'is' instead of 'GetType' so that it works for inherited types too
    public override bool CanAttack(EntityOLD entity)
    {
        return base.CanAttack(entity) &&
               (entity is Monster ||
                entity is PlayerOLD ||
                (entity is Pet && entity != petControl.activePet) ||
                (entity is Mount && entity != mountControl.activeMount));
    }

    // pvp murder system ///////////////////////////////////////////////////////
    // attacking someone innocent results in Offender status
    //   (can be attacked without penalty for a short time)
    // killing someone innocent results in Murderer status
    //   (can be attacked without penalty for a long time + negative buffs)
    // attacking/killing a Offender/Murderer has no penalty
    //
    // we use buffs for the offender/status because buffs have all the features
    // that we need here.
    //
    // NOTE: this is in Player.cs and not in PlayerCombat.cs for ease of use!
    public bool IsOffender()
    {
        return offenderBuff != null && skills.GetBuffIndexByName(offenderBuff.name) != -1;
    }

    public bool IsMurderer()
    {
        return murdererBuff != null && skills.GetBuffIndexByName(murdererBuff.name) != -1;
    }

    public bool IsInnocent()
    {
        return !IsOffender() && !IsMurderer();
    }

    public void StartOffender()
    {
        if (offenderBuff != null) skills.AddOrRefreshBuff(new Buff(offenderBuff, 1));
    }

    public void StartMurderer()
    {
        if (murdererBuff != null) skills.AddOrRefreshBuff(new Buff(murdererBuff, 1));
    }

    // selection handling //////////////////////////////////////////////////////
    [Command]
    public void CmdSetTarget(NetworkIdentity ni)
    {
        // validate
        if (ni != null)
        {
            // can directly change it, or change it after casting?
            if (state == "IDLE" || state == "MOVING" || state == "STUNNED")
                target = ni.GetComponent<EntityOLD>();
            else if (state == "CASTING")
                nextTarget = ni.GetComponent<EntityOLD>();
        }
    }

    // interaction /////////////////////////////////////////////////////////////
    protected override void OnInteract()
    {
        // not local player?
        if (this != localPlayer)
        {
            // attackable and has skills? => attack
            if (localPlayer.CanAttack(this) && localPlayer.skills.skills.Count > 0)
            {
                // then try to use that one
                ((PlayerSkills)localPlayer.skills).TryUse(0);
            }
            // otherwise just walk there
            // (e.g. if clicking on it in a safe zone where we can't attack)
            else
            {
                // use collider point(s) to also work with big entities
                Vector3 destination = Utils.ClosestPoint(this, localPlayer.transform.position);
                localPlayer.movement.Navigate(destination, localPlayer.interactionRange);
            }
        }
    }
}
