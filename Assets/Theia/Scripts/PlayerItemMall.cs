using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[Serializable]
public partial struct ItemMallCategory
{
    public string category;
    public ScriptableItem[] items;
}

[RequireComponent(typeof(PlayerChat))]
[RequireComponent(typeof(PlayerInventory))]
[DisallowMultipleComponent]
public class PlayerItemMall : NetworkBehaviour
{
    [Header("Components")]
    public Player player;
    public PlayerChat chat;
    public PlayerInventory inventory;

    [Header("Item Mall")]
    public ScriptableItemMall config;
    [SyncVar] public long coins = 0;
    public float couponWaitSeconds = 3;

    public override void OnStartServer()
    {
        InvokeRepeating(nameof(ProcessCoinOrders), 5, 5);
    }

    // item mall ///////////////////////////////////////////////////////////////
    [Command]
    public void CmdEnterCoupon(string coupon)
    {
        // only allow entering one coupon every few seconds to avoid brute force
        if (NetworkTime.time >= player.nextRiskyActionTime)
        {
            // YOUR COUPON VALIDATION CODE HERE
            // coins += ParseCoupon(coupon);
            Debug.Log("coupon: " + coupon + " => " + name + "@" + NetworkTime.time);
            player.nextRiskyActionTime = NetworkTime.time + couponWaitSeconds;
        }
    }

    [Command]
    public void CmdUnlockItem(int categoryIndex, int itemIndex)
    {
        // validate: only if alive so people can't buy resurrection potions
        // after dieing in a PvP fight etc.
        if (player.health.current > 0 &&
            0 <= categoryIndex && categoryIndex <= config.categories.Length &&
            0 <= itemIndex && itemIndex <= config.categories[categoryIndex].items.Length)
        {
            Item item = new Item(config.categories[categoryIndex].items[itemIndex]);
            if (0 < item.itemMallPrice && item.itemMallPrice <= coins)
            {
                // try to add it to the inventory, subtract costs from coins
                if (inventory.Add(item, 1))
                {
                    coins -= item.itemMallPrice;
                    Debug.Log(name + " unlocked " + item.name);

                    // NOTE: item mall purchases need to be persistent, yet
                    // resaving the player here is not necessary because if the
                    // server crashes before next save, then both the inventory
                    // and the coins will be reverted anyway.
                }
            }
        }
    }

    // coins can't be increased by an external application while the player is
    // ingame. we use an additional table to store new orders in and process
    // them every few seconds from here. this way we can even notify the player
    // after his order was processed successfully.
    //
    // note: the alternative is to keep player.coins in the database at all
    // times, but then we need RPCs and the client needs a .coins value anyway.
    [Server]
    void ProcessCoinOrders()
    {
        List<long> orders = Database.singleton.GrabCharacterOrders(name);
        foreach (long reward in orders)
        {
            coins += reward;
            Debug.Log("Processed order for: " + name + ";" + reward);
            chat.TargetMsgInfo("Processed order for: " + reward);
        }
    }
}
