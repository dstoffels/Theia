using UnityEngine;
using Mirror;
using TMPro;

[RequireComponent(typeof(Experience))]
[RequireComponent(typeof(PetSkills))]
[RequireComponent(typeof(NavMeshMovement))]
[RequireComponent(typeof(NetworkNavMeshAgent))]
public partial class Pet : Summonable
{
    [Header("Components")]
    public Experience experience;

    [Header("Icons")]
    public Sprite portraitIcon; // for pet status UI

    [Header("Text Meshes")]
    public TextMeshPro ownerNameOverlay;

    // use owner's speed if found, so that the pet can still follow the
    // owner if he is riding a mount, etc.
    public override float speed => owner != null ? owner.speed : base.speed;

    [Header("Death")]
    public float deathTime = 2; // enough for animation
    [HideInInspector] public double deathTimeEnd; // double for long term precision

    [Header("Behaviour")]
    [SyncVar] public bool defendOwner = true; // attack what attacks the owner
    [SyncVar] public bool autoAttack = true; // attack what the owner attacks

    // sync to item ////////////////////////////////////////////////////////////
    protected override ItemSlot SyncStateToItemSlot(ItemSlot slot)
    {
        // pet also has experience, unlike summonable. sync that too.
        slot = base.SyncStateToItemSlot(slot);
        slot.item.summonedExperience = experience.current;
        return slot;
    }

    // networkbehaviour ////////////////////////////////////////////////////////
    void LateUpdate()
    {
        // pass parameters to animation state machine
        // => passing the states directly is the most reliable way to avoid all
        //    kinds of glitches like movement sliding, attack twitching, etc.
        // => make sure to import all looping animations like idle/run/attack
        //    with 'loop time' enabled, otherwise the client might only play it
        //    once
        // => only play moving animation while actually moving (velocity). the
        //    MOVING state might be delayed to due latency or we might be in
        //    MOVING while a path is still pending, etc.
        // => skill names are assumed to be boolean parameters in animator
        //    so we don't need to worry about an animation number etc.
        if (isClient) // no need for animations on the server
        {
            animator.SetBool("MOVING", state == "MOVING" && movement.GetVelocity() != Vector3.zero);
            animator.SetBool("CASTING", state == "CASTING");
            animator.SetBool("STUNNED", state == "STUNNED");
            animator.SetBool("DEAD", state == "DEAD");
            foreach (SkillOLD skill in skills.skills)
                animator.SetBool(skill.name, skill.CastTimeRemaining() > 0);
        }

        // update overlays in any case, except on server-only mode
        // (also update for character selection previews etc. then)
        if (!isServerOnly)
        {
            if (ownerNameOverlay != null)
            {
                if (owner != null)
                {
                    ownerNameOverlay.text = owner.name;
                    // find local player (null while in character selection)
                    if (PlayerOLD.localPlayer != null)
                    {

                        // note: murderer has higher priority (a player can be a murderer and an
                        // offender at the same time)
                        if (owner.IsMurderer())
                            ownerNameOverlay.color = owner.nameOverlayMurdererColor;
                        else if (owner.IsOffender())
                            ownerNameOverlay.color = owner.nameOverlayOffenderColor;
                        // member of the same party
                        else if (PlayerOLD.localPlayer.party.InParty() &&
                                 PlayerOLD.localPlayer.party.party.Contains(owner.name))
                            ownerNameOverlay.color = owner.nameOverlayPartyColor;
                        // otherwise default
                        else
                            ownerNameOverlay.color = owner.nameOverlayDefaultColor;
                    }
                }
                else ownerNameOverlay.text = "?";
            }
        }
    }

    void OnDestroy()
    {
        // keep player's pet item up to date
        if (isServer)
            SyncToOwnerItem();
    }

    // always update pets. IsWorthUpdating otherwise only updates if has observers,
    // but pets should still be updated even if they are too far from any observers,
    // so that they continue to run to their owner.
    public override bool IsWorthUpdating() { return true; }

    // aggro ///////////////////////////////////////////////////////////////////
    // this function is called by entities that attack us
    [ServerCallback]
    public override void OnAggro(EntityOLD entity)
    {
        // call base function
        base.OnAggro(entity);

        // are we alive, and is the entity alive and of correct type?
        if (CanAttack(entity))
        {
            // no target yet(==self), or closer than current target?
            // => has to be at least 20% closer to be worth it, otherwise we
            //    may end up nervously switching between two targets
            // => we do NOT use Utils.ClosestDistance, because then we often
            //    also end up nervously switching between two animated targets,
            //    since their collides moves with the animation.
            if (target == null)
            {
                target = entity;
            }
            else
            {
                float oldDistance = Vector3.Distance(transform.position, target.transform.position);
                float newDistance = Vector3.Distance(transform.position, entity.transform.position);
                if (newDistance < oldDistance * 0.8) target = entity;
            }
        }
    }

    // death ///////////////////////////////////////////////////////////////////
    [Server]
    public override void OnDeath()
    {
        // take care of entity stuff
        base.OnDeath();

        // set death end time. we set it now to make sure that everything works
        // fine even if a pet isn't updated for a while. so as soon as it's
        // updated again, the death/respawn will happen immediately if current
        // time > end time.
        deathTimeEnd = NetworkTime.time + deathTime;

        // keep player's pet item up to date
        SyncToOwnerItem();
    }

    // attack //////////////////////////////////////////////////////////////////
    // CanAttack check
    // we use 'is' instead of 'GetType' so that it works for inherited types too
    public override bool CanAttack(EntityOLD entity)
    {
        return base.CanAttack(entity) &&
               (entity is Monster ||
                (entity is PlayerOLD && entity != owner) ||
                (entity is Pet pet && pet.owner != owner) ||
                (entity is Mount mount && mount.owner != owner));
    }

    // owner controls //////////////////////////////////////////////////////////
    // the Commands can only be called by the owner connection, so make sure to
    // spawn the pet via NetworkServer.Spawn(prefab, ownerConnection).
    [Command]
    public void CmdSetAutoAttack(bool value)
    {
        autoAttack = value;
    }

    [Command]
    public void CmdSetDefendOwner(bool value)
    {
        defendOwner = value;
    }

    // interaction /////////////////////////////////////////////////////////////
    protected override void OnInteract()
    {
        PlayerOLD player = PlayerOLD.localPlayer;

        // not local player's pet?
        if (this != player.petControl.activePet)
        {
            // attackable and has skills? => attack
            if (player.CanAttack(this) && player.skills.skills.Count > 0)
            {
                // then try to use that one
                ((PlayerSkills)player.skills).TryUse(0);
            }
            // otherwise just walk there
            // (e.g. if clicking on it in a safe zone where we can't attack)
            else
            {
                // use collider point(s) to also work with big entities
                Vector3 destination = Utils.ClosestPoint(this, player.transform.position);
                player.movement.Navigate(destination, player.interactionRange);
            }
        }
    }
}
