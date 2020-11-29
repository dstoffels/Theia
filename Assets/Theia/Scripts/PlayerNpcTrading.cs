using UnityEngine;
using Mirror;

[RequireComponent(typeof(PlayerInventory))]
[DisallowMultipleComponent]
public class PlayerNpcTrading : NetworkBehaviour
{
    [Header("Components")]
    public Player player;
    public PlayerInventory inventory;

    // trading /////////////////////////////////////////////////////////////////
    [Command]
    public void CmdBuyItem(int index, int amount)
    {
        // validate: close enough, npc alive and valid index?
        // use collider point(s) to also work with big entities
        if (player.state == "IDLE" &&
            player.target != null &&
            player.target.health.current > 0 &&
            player.target is Npc npc &&
            npc.trading != null && // only if Npc offers trading
            Utils.ClosestDistance(player, npc) <= player.interactionRange &&
            0 <= index && index < npc.trading.saleItems.Length)
        {
            // valid amount?
            Item npcItem = new Item(npc.trading.saleItems[index]);
            if (1 <= amount && amount <= npcItem.maxStack)
            {
                long price = npcItem.buyPrice * amount;

                // enough gold and enough space in inventory?
                if (player.gold >= price && inventory.CanAdd(npcItem, amount))
                {
                    // pay for it, add to inventory
                    player.gold -= price;
                    inventory.Add(npcItem, amount);
                }
            }
        }
    }

    [Command]
    public void CmdSellItem(int index, int amount)
    {
        // validate: close enough, npc alive and valid index and valid item?
        // use collider point(s) to also work with big entities
        if (player.state == "IDLE" &&
            player.target != null &&
            player.target.health.current > 0 &&
            player.target is Npc npc &&
            npc.trading != null && // only if Npc offers trading
            Utils.ClosestDistance(player, player.target) <= player.interactionRange &&
            0 <= index && index < inventory.slots.Count)
        {
            // sellable?
            ItemSlot slot = inventory.slots[index];
            if (slot.amount > 0 && slot.item.sellable && !slot.item.summoned)
            {
                // valid amount?
                if (1 <= amount && amount <= slot.amount)
                {
                    // sell the amount
                    long price = slot.item.sellPrice * amount;
                    player.gold += price;
                    slot.DecreaseAmount(amount);
                    inventory.slots[index] = slot;
                }
            }
        }
    }

    [Command]
    public void CmdRepairAllItems()
    {
        // validate: close enough, npc alive and valid index and valid item?
        // use collider point(s) to also work with big entities
        if (player.state == "IDLE" &&
            player.target != null &&
            player.target.health.current > 0 &&
            player.target is Npc npc &&
            npc.trading != null && // only if Npc offers trading
            npc.trading.offersRepair && // and repairs
            Utils.ClosestDistance(player, player.target) <= player.interactionRange)
        {
            // calculate missing durability from inventory + equipment
            int missing = player.inventory.GetTotalMissingDurability() +
                          player.equipment.GetTotalMissingDurability();

            // calculate costs based on npc repair costs
            int price = missing * npc.trading.repairCostPerDurabilityPoint;

            // don't allow negative price ever. just in case a calculation is
            // wrong, we don't want the player to get money back when repairing.
            if (price > 0)
            {
                // check if player has enough gold
                if (player.gold >= price)
                {
                    // repair all items
                    player.inventory.RepairAllItems();
                    player.equipment.RepairAllItems();

                    // reduce gold
                    player.gold -= price;
                }
            }
        }
    }

    // drag & drop /////////////////////////////////////////////////////////////
    void OnDragAndDrop_InventorySlot_NpcSellSlot(int[] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        ItemSlot slot = inventory.slots[slotIndices[0]];
        if (slot.item.sellable && !slot.item.summoned)
        {
            UINpcTrading.singleton.sellIndex = slotIndices[0];
            UINpcTrading.singleton.sellAmountInput.text = slot.amount.ToString();
        }
    }

    void OnDragAndClear_NpcSellSlot(int slotIndex)
    {
        UINpcTrading.singleton.sellIndex = -1;
    }
}
