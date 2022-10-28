using UnityEngine;
using Mirror;

[RequireComponent(typeof(PlayerInventory))]
[DisallowMultipleComponent]
public class PlayerPetControl : NetworkBehaviour
{
    [Header("Components")]
    public PlayerOLD player;
    public PlayerInventory inventory;

    [Header("Pet")]
    [SyncVar, HideInInspector] public Pet activePet;

    // pet's destination should always be right next to player, not inside him
    // -> we use a helper property so we don't have to recalculate it each time
    // -> we offset the position by exactly 1 x bounds to the left because dogs
    //    are usually trained to walk on the left of the owner. looks natural.
    public Vector3 petDestination
    {
        get
        {
            Bounds bounds = player.collider.bounds;
            return transform.position - transform.right * bounds.size.x;
        }
    }

    // pet /////////////////////////////////////////////////////////////////////
    // helper function for command and UI
    public bool CanUnsummon()
    {
        // only while pet and owner aren't fighting
        return activePet != null &&
               (   player.state == "IDLE" ||    player.state == "MOVING") &&
               (activePet.state == "IDLE" || activePet.state == "MOVING");
    }

    [Command]
    public void CmdUnsummon()
    {
        // validate
        if (CanUnsummon())
        {
            // destroy from world. item.summoned and activePet will be null.
            NetworkServer.Destroy(activePet.gameObject);
        }
    }

    // combat //////////////////////////////////////////////////////////////////
    [Server]
    public void OnDamageDealtTo(EntityOLD victim)
    {
        // let pet know that we attacked something
        if (activePet != null && activePet.autoAttack)
            activePet.OnAggro(victim);
    }

    [Server]
    public void OnKilledEnemy(EntityOLD victim)
    {
        // killed a monster
        if (victim is Monster monster)
        {
            // give pet the experience without sharing it in party or similar,
            // but balance it
            // => AFTER player exp reward! pet can only ever level up to player
            //    level, so it's best if the player gets exp and level-ups
            //    first, then afterwards we try to level up the pet.
            if (activePet != null)
            {
                activePet.experience.current += Experience.BalanceExperienceReward(monster.rewardExperience, activePet.level.current, victim.level.current);
                // sync to owner item so the progress is saved, e.g. on level up
                // we don't sync all the time, but here it makes sense.
                // see also: https://github.com/vis2k/uMMORPG_CE/issues/35
                activePet.SyncToOwnerItem();
            }
        }
    }

    // drag & drop /////////////////////////////////////////////////////////////
    void OnDragAndDrop_InventorySlot_NpcReviveSlot(int[] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        if (inventory.slots[slotIndices[0]].item.data is SummonableItem)
            UINpcRevive.singleton.itemIndex = slotIndices[0];
    }

    void OnDragAndClear_NpcReviveSlot(int slotIndex)
    {
        UINpcRevive.singleton.itemIndex = -1;
    }
}
