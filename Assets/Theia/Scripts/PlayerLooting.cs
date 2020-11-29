using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(PlayerInventory))]
[RequireComponent(typeof(PlayerParty))]
[DisallowMultipleComponent]
public class PlayerLooting : NetworkBehaviour
{
    [Header("Components")]
    public Player player;
    public PlayerInventory inventory;
    public PlayerParty party;

    // loot ////////////////////////////////////////////////////////////////////
    [Command]
    public void CmdTakeGold()
    {
        // validate: dead monster and close enough?
        // use collider point(s) to also work with big entities
        if ((player.state == "IDLE" || player.state == "MOVING" || player.state == "CASTING") &&
            player.target != null &&
            player.target is Monster &&
            player.target.health.current == 0 &&
            Utils.ClosestDistance(player, player.target) <= player.interactionRange)
        {
            // distribute reward through party or to self
            if (party.InParty() && party.party.shareGold)
            {
                // find all party members in observer range
                // (we don't distribute it all across the map. standing
                //  next to each other is a better experience. players
                //  can't just stand safely in a city while gaining exp)
                List<Player> closeMembers = party.GetMembersInProximity();

                // calculate the share via ceil, so that uneven numbers
                // still result in at least total gold in the end.
                // e.g. 4/2=2 (good); 5/2=2 (1 gold got lost)
                long share = (long)Mathf.Ceil((float)player.target.gold / (float)closeMembers.Count);

                // now distribute
                foreach (Player member in closeMembers)
                    member.gold += share;
            }
            else
            {
                player.gold += player.target.gold;
            }

            // reset target gold
            player.target.gold = 0;
        }
    }

    [Command]
    public void CmdTakeItem(int index)
    {
        // validate: dead monster and close enough and valid loot index?
        // use collider point(s) to also work with big entities
        if ((player.state == "IDLE" || player.state == "MOVING" || player.state == "CASTING") &&
            player.target != null &&
            player.target is Monster monster &&
            player.target.health.current == 0 &&
            Utils.ClosestDistance(player, player.target) <= player.interactionRange)
        {
            if (0 <= index && index < monster.inventory.slots.Count &&
                monster.inventory.slots[index].amount > 0)
            {
                ItemSlot slot = monster.inventory.slots[index];

                // try to add it to the inventory, clear monster slot if it worked
                if (inventory.Add(slot.item, slot.amount))
                {
                    slot.amount = 0;
                    monster.inventory.slots[index] = slot;
                }
            }
        }
    }
}
