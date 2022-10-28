// how trading works:
// 1. A invites his target with CmdTradeRequest()
//    -> sets B.tradeInvitationFrom = A;
// 2. B sees a UI window and accepts (= invites A too)
//    -> sets A.tradeInvitationFrom = B;
// 3. the TradeStart event is fired, both go to 'TRADING' state
// 4. they lock the trades
// 5. they accept, then items and gold are swapped
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public enum TradingState { Free, Locked, Accepted }

[RequireComponent(typeof(PlayerOLD))]
[RequireComponent(typeof(PlayerInventory))]
[DisallowMultipleComponent]
public class PlayerTrading : NetworkBehaviour
{
    [Header("Components")]
    public PlayerOLD player;
    public PlayerInventory inventory;

    [Header("Trading")]
    [SyncVar, HideInInspector] public string requestFrom = "";
    [SyncVar, HideInInspector] public TradingState state = TradingState.Free;
    [SyncVar, HideInInspector] public long offerGold = 0;
    public SyncList<int> offerItems = new SyncList<int>(); // inventory indices

    public override void OnStartServer()
    {
        // initialize trade item indices
        for (int i = 0; i < 6; ++i)
            offerItems.Add(-1);
    }

    // player to player trading ////////////////////////////////////////////////
    public bool CanStartTrade()
    {
        // a player can only trade if he is not trading already and alive
        return player.health.current > 0 && player.state != "TRADING";
    }

    public bool CanStartTradeWith(EntityOLD entity)
    {
        // can we trade? can the target trade? are we close enough?
        return entity != null &&
               entity is PlayerOLD other &&
               other != player &&
               CanStartTrade() &&
               other.trading.CanStartTrade() &&
               Utils.ClosestDistance(player, entity) <= player.interactionRange;
    }

    // request a trade with the target player.
    [Command]
    public void CmdSendRequest()
    {
        // validate
        if (CanStartTradeWith(player.target))
        {
            // send a trade request to target
            ((PlayerOLD)player.target).trading.requestFrom = name;
            Debug.Log(name + " invited " + player.target.name + " to trade");
        }
    }

    // helper function to find the guy who sent us a trade invitation
    [Server]
    public PlayerOLD FindPlayerFromInvitation()
    {
        if (requestFrom != "" &&
            PlayerOLD.onlinePlayers.TryGetValue(requestFrom, out PlayerOLD sender))
        {
            return sender;
        }
        return null;
    }

    // accept a trade invitation by simply setting 'requestFrom' for the other
    // person to self
    [Command]
    public void CmdAcceptRequest()
    {
        PlayerOLD sender = FindPlayerFromInvitation();
        if (sender != null)
        {
            if (CanStartTradeWith(sender))
            {
                // also send a trade request to the person that invited us
                sender.trading.requestFrom = name;
                Debug.Log(name + " accepted " + sender.name + "'s trade request");
            }
        }
    }

    // decline a trade invitation
    [Command]
    public void CmdDeclineRequest()
    {
        requestFrom = "";
    }

    [Server]
    public void Cleanup()
    {
        // clear all trade related properties
        offerGold = 0;
        for (int i = 0; i < offerItems.Count; ++i)
            offerItems[i] = -1;
        state = TradingState.Free;
        requestFrom = "";
    }

    [Command]
    public void CmdCancel()
    {
        // validate
        if (player.state == "TRADING")
        {
            // clear trade request for both guys. the FSM event will do the rest
            PlayerOLD other = FindPlayerFromInvitation();
            if (other != null)
                other.trading.requestFrom = "";
            requestFrom = "";
        }
    }

    [Command]
    public void CmdLockOffer()
    {
        // validate
        if (player.state == "TRADING")
            state = TradingState.Locked;
    }

    [Command]
    public void CmdOfferGold(long amount)
    {
        // validate
        if (player.state == "TRADING" && state == TradingState.Free &&
            0 <= amount && amount <= player.gold)
            offerGold = amount;
    }

    [Command]
    public void CmdOfferItem(int inventoryIndex, int offerIndex)
    {
        // validate
        if (player.state == "TRADING" && state == TradingState.Free &&
            0 <= offerIndex && offerIndex < offerItems.Count &&
            !offerItems.Contains(inventoryIndex) && // only one reference
            0 <= inventoryIndex && inventoryIndex < inventory.slots.Count)
        {
            ItemSlot slot = inventory.slots[inventoryIndex];
            if (slot.amount > 0 && slot.item.tradable && !slot.item.summoned)
                offerItems[offerIndex] = inventoryIndex;
        }
    }

    [Command]
    public void CmdClearOfferItem(int offerIndex)
    {
        // validate
        if (player.state == "TRADING" && state == TradingState.Free &&
            0 <= offerIndex && offerIndex < offerItems.Count)
            offerItems[offerIndex] = -1;
    }

    bool IsInventorySlotTradable(int index)
    {
        return 0 <= index && index < inventory.slots.Count &&
               inventory.slots[index].amount > 0 &&
               inventory.slots[index].item.tradable;
    }

    [Server]
    bool IsOfferStillValid()
    {
        // not enough gold? then invalid
        if (player.gold < offerGold)
            return false;

        // all offered items are -1 or valid?
        // (avoid Linq because it is HEAVY(!) on GC and performance)
        foreach (int index in offerItems)
        {
            if (index == -1 || IsInventorySlotTradable(index))
            {
                // good
            }
            else
            {
                // invalid item
                return false;
            }
        }
        return true;
    }

    [Server]
    int OfferItemSlotAmount()
    {
        // (avoid Linq because it is HEAVY(!) on GC and performance)
        int count = 0;
        foreach (int index in offerItems)
            if (index != -1)
                ++count;
        return count;
    }

    [Server]
    int InventorySlotsNeededForTrade()
    {
        // if other guy offers 2 items and we offer 1 item then we only need
        // 2-1 = 1 slots. and the other guy would need 1-2 slots and at least 0.
        if (player.target != null &&
            player.target is PlayerOLD other)
        {
            int otherAmount = other.trading.OfferItemSlotAmount();
            int myAmount = OfferItemSlotAmount();
            return Mathf.Max(otherAmount - myAmount, 0);
        }
        return 0;
    }

    [Command]
    public void CmdAcceptOffer()
    {
        // validate
        // note: distance check already done when starting the trade
        if (player.state == "TRADING" && state == TradingState.Locked &&
            player.target != null &&
            player.target is PlayerOLD other)
        {
            // other has locked?
            if (other.trading.state == TradingState.Locked)
            {
                //  simply accept and wait for the other guy to accept too
                state = TradingState.Accepted;
                Debug.Log("first accept by " + name);
            }
            // other has accepted already? then both accepted now, start trade.
            else if (other.trading.state == TradingState.Accepted)
            {
                // accept
                state = TradingState.Accepted;
                Debug.Log("second accept by " + name);

                // both offers still valid?
                if (IsOfferStillValid() && other.trading.IsOfferStillValid())
                {
                    // both have enough inventory slots?
                    // note: we don't use InventoryCanAdd here because:
                    // - current solution works if both have full inventories
                    // - InventoryCanAdd only checks one slot. here we have
                    //   multiple slots though (it could happen that we can
                    //   not add slot 2 after we did add slot 1's items etc)
                    if (inventory.SlotsFree() >= InventorySlotsNeededForTrade() &&
                        other.inventory.SlotsFree() >= other.trading.InventorySlotsNeededForTrade())
                    {
                        // exchange the items by first taking them out
                        // into a temporary list and then putting them
                        // in. this guarantees that exchanging even
                        // works with full inventories

                        // take them out
                        Queue<ItemSlot> tempMy = new Queue<ItemSlot>();
                        foreach (int index in offerItems)
                        {
                            if (index != -1)
                            {
                                ItemSlot slot = inventory.slots[index];
                                tempMy.Enqueue(slot);
                                slot.amount = 0;
                                inventory.slots[index] = slot;
                            }
                        }

                        Queue<ItemSlot> tempOther = new Queue<ItemSlot>();
                        foreach (int index in other.trading.offerItems)
                        {
                            if (index != -1)
                            {
                                ItemSlot slot = other.inventory.slots[index];
                                tempOther.Enqueue(slot);
                                slot.amount = 0;
                                other.inventory.slots[index] = slot;
                            }
                        }

                        // put them into the free slots
                        for (int i = 0; i < inventory.slots.Count; ++i)
                            if (inventory.slots[i].amount == 0 && tempOther.Count > 0)
                                inventory.slots[i] = tempOther.Dequeue();

                        for (int i = 0; i < other.inventory.slots.Count; ++i)
                            if (other.inventory.slots[i].amount == 0 && tempMy.Count > 0)
                                other.inventory.slots[i] = tempMy.Dequeue();

                        // did it all work?
                        if (tempMy.Count > 0 || tempOther.Count > 0)
                            Debug.LogWarning("item trade problem");

                        // exchange the gold
                        player.gold -= offerGold;
                        other.gold -= other.trading.offerGold;

                        player.gold += other.trading.offerGold;
                        other.gold += offerGold;
                    }
                }
                else Debug.Log("trade canceled (invalid offer)");

                // clear trade request for both guys. the FSM event will do the
                // rest
                requestFrom = "";
                other.trading.requestFrom = "";
            }
        }
    }

    // drag & drop /////////////////////////////////////////////////////////////
    void OnDragAndDrop_InventorySlot_TradingSlot(int[] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        if (inventory.slots[slotIndices[0]].item.tradable)
            CmdOfferItem(slotIndices[0], slotIndices[1]);
    }

    void OnDragAndClear_TradingSlot(int slotIndex)
    {
        CmdClearOfferItem(slotIndex);
    }
}
