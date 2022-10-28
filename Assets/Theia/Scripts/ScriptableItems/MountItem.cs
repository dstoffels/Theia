using UnityEngine;
using Mirror;

[CreateAssetMenu(menuName="uMMORPG Item/Mount", order=999)]
public class MountItem : SummonableItem
{
    // usage
    public override bool CanUse(PlayerOLD player, int inventoryIndex)
    {
        // summonable checks if we can summon it already,
        // we just need to check if we have no active mount summoned yet
        // OR if this is the active mount, so we unsummon it
        ItemOLD item = player.inventory.slots[inventoryIndex].item;
        return base.CanUse(player, inventoryIndex) &&
               (player.mountControl.activeMount == null ||
                player.mountControl.activeMount.netIdentity == item.summoned);
    }

    public override void Use(PlayerOLD player, int inventoryIndex)
    {
        // always call base function too
        base.Use(player, inventoryIndex);

        // summon
        if (player.mountControl.activeMount == null)
        {
            // summon at player position
            ItemSlot slot = player.inventory.slots[inventoryIndex];
            GameObject go = Instantiate(summonPrefab.gameObject, player.transform.position, player.transform.rotation);
            Mount mount = go.GetComponent<Mount>();
            mount.name = summonPrefab.name; // avoid "(Clone)"
            mount.owner = player;
            mount.health.current = slot.item.summonedHealth;

            // spawn with owner connection so the owner can call [Command]s on it
            NetworkServer.Spawn(go, player.connectionToClient);
            player.mountControl.activeMount = mount; // set syncvar after spawning

            // set item summoned pet reference so we know it can't be sold etc.
            slot.item.summoned = go.GetComponent<NetworkIdentity>();
            player.inventory.slots[inventoryIndex] = slot;
        }
        // unsummon
        else
        {
            // destroy from world. item.summoned and activePet will be null.
            NetworkServer.Destroy(player.mountControl.activeMount.gameObject);
        }
    }
}
