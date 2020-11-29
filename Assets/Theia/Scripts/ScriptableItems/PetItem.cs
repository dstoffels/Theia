using UnityEngine;
using Mirror;

[CreateAssetMenu(menuName="uMMORPG Item/Pet", order=999)]
public class PetItem : SummonableItem
{
    // usage
    public override bool CanUse(Player player, int inventoryIndex)
    {
        // summonable checks if we can summon it already,
        // we just need to check if we have no active pet summoned yet
        return base.CanUse(player, inventoryIndex) &&
               player.petControl.activePet == null;
    }

    public override void Use(Player player, int inventoryIndex)
    {
        // always call base function too
        base.Use(player, inventoryIndex);

        // summon right next to the player
        ItemSlot slot = player.inventory.slots[inventoryIndex];
        GameObject go = Instantiate(summonPrefab.gameObject, player.petControl.petDestination, Quaternion.identity);
        Pet pet = go.GetComponent<Pet>();
        pet.name = summonPrefab.name; // avoid "(Clone)"
        pet.owner = player;
        pet.level.current = slot.item.summonedLevel;
        pet.experience.current = slot.item.summonedExperience;
        // set health AFTER level, otherwise health is always level 1 max health
        pet.health.current = slot.item.summonedHealth;

        // spawn with owner connection so the owner can call [Command]s on it
        NetworkServer.Spawn(go, player.connectionToClient);
        player.petControl.activePet = pet; // set syncvar after spawning

        // set item summoned pet reference so we know it can't be sold etc.
        slot.item.summoned = go.GetComponent<NetworkIdentity>();
        player.inventory.slots[inventoryIndex] = slot;
    }
}
