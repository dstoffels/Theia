// a portal that teleports a player into a dungeon instance for his party
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PortalToInstance : MonoBehaviour
{
    [Tooltip("Instance template in the Scene. Don't use a prefab, Mirror can't handle prefabs that contain NetworkIdentity children.")]
    public Instance instanceTemplate;

    void OnPortal(PlayerOLD player)
    {
        // check party again, just to be sure.
        if (player.party.InParty())
        {
            // is there an instance for the player's party yet?
            if (instanceTemplate.instances.TryGetValue(player.party.party.partyId, out Instance existingInstance))
            {
                // teleport player to instance entry
                if (player.isServer) player.movement.Warp(existingInstance.entry.position);
                Debug.Log("Teleporting " + player.name + " to existing instance=" + existingInstance.name + " with partyId=" + player.party.party.partyId);
            }
            // otherwise create a new one
            else
            {
                Instance instance = Instance.CreateInstance(instanceTemplate, player.party.party.partyId);
                if (instance != null)
                {
                    // teleport player to instance entry
                    if (player.isServer) player.movement.Warp(instance.entry.position);
                    Debug.Log("Teleporting " + player.name + " to new instance=" + instance.name + " with partyId=" + player.party.party.partyId);
                }
                else if (player.isServer) player.chat.TargetMsgInfo("There are already too many " + instanceTemplate.name + " instances. Please try again later.");
            }
        }
    }

    void OnTriggerEnter(Collider co)
    {
        if (instanceTemplate != null)
        {
            // collider might be in player's bone structure. look in parents.
            PlayerOLD player = co.GetComponentInParent<PlayerOLD>();
            if (player != null)
            {
                // only call this for server and for local player. not for other
                // players on the client. no need in locally creating their
                // instances too.
                if (player.isServer || player.isLocalPlayer)
                {
                    // required level?
                    if (player.level.current >= instanceTemplate.requiredLevel)
                    {
                        // can only enter with a party
                        if (player.party.InParty())
                        {
                            // call OnPortal on server and on local client
                            OnPortal(player);
                        }
                        // show info message on client directly. no need to do it via Rpc.
                        else if (player.isClient)
                            player.chat.AddMsgInfo("Can't enter instance without a party.");
                    }
                    // show info message on client directly. no need to do it via Rpc.
                    else if (player.isClient)
                        player.chat.AddMsgInfo("Portal requires level " + instanceTemplate.requiredLevel);
                }
            }
        }
    }
}
