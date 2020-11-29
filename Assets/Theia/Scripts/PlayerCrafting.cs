using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;

public enum CraftingState { None, InProgress, Success, Failed }

[RequireComponent(typeof(PlayerInventory))]
[DisallowMultipleComponent]
public class PlayerCrafting : NetworkBehaviour
{
    [Header("Components")]
    public Player player;
    public PlayerInventory inventory;

    [Header("Crafting")]
    public List<int> indices = Enumerable.Repeat(-1, ScriptableRecipe.recipeSize).ToList();
    [HideInInspector] public CraftingState state = CraftingState.None; // // client sided
    ScriptableRecipe currentRecipe; // currently crafted recipe. cached to avoid searching ALL recipes in Craft()
    [SyncVar, HideInInspector] public double endTime; // double for long term precision
    [HideInInspector] public bool requestPending; // for state machine event

    // crafting ////////////////////////////////////////////////////////////////
    // the crafting system is designed to work with all kinds of commonly known
    // crafting options:
    // - item combinations: wood + stone = axe
    // - weapon upgrading: axe + gem = strong axe
    // - recipe items: axerecipe(item) + wood(item) + stone(item) = axe(item)
    //
    // players can craft at all times, not just at npcs, because that's the most
    // realistic option

    // craft a recipe with the combination of items and put result into inventory
    // => we pass the recipe name so that we don't have to search ALL the
    //    recipes. this would slow down the server if we have lots of recipes.
    // => we just let the client do the searching!
    [Command]
    public void CmdCraft(string recipeName, int[] clientIndices)
    {
        // validate: between 1 and 6, all valid, no duplicates?
        // -> can be IDLE or MOVING (in which case we reset the movement)
        if ((player.state == "IDLE" || player.state == "MOVING") &&
            clientIndices.Length == ScriptableRecipe.recipeSize)
        {
            // find valid indices that are not '-1' and make sure there are no
            // duplicates
            List<int> validIndices = clientIndices.Where(index => 0 <= index && index < inventory.slots.Count && inventory.slots[index].amount > 0).ToList();
            if (validIndices.Count > 0 && !validIndices.HasDuplicates())
            {
                // find recipe
                if (ScriptableRecipe.All.TryGetValue(recipeName, out ScriptableRecipe recipe) &&
                    recipe.result != null)
                {
                    // enough space?
                    Item result = new Item(recipe.result);
                    if (inventory.CanAdd(result, 1))
                    {
                        // cache recipe so we don't have to search for it again
                        // in Craft()
                        currentRecipe = recipe;

                        // store the crafting indices on the server. no need for
                        // a SyncList and unnecessary broadcasting.
                        // we already have a 'craftingIndices' variable anyway.
                        indices = clientIndices.ToList();

                        // start crafting
                        requestPending = true;
                        endTime = NetworkTime.time + recipe.craftingTime;
                    }
                }
            }
        }
    }

    // finish the crafting
    [Server]
    public void Craft()
    {
        // should only be called while CRAFTING and if recipe still valid
        // (no one should touch 'craftingRecipe', but let's just be sure.
        // -> we already validated everything in CmdCraft. let's just craft.
        if (player.state == "CRAFTING" &&
            currentRecipe != null &&
            currentRecipe.result != null)
        {
            // enough space?
            Item result = new Item(currentRecipe.result);
            if (inventory.CanAdd(result, 1))
            {
                // remove the ingredients from inventory in any case
                foreach (ScriptableItemAndAmount ingredient in currentRecipe.ingredients)
                    if (ingredient.amount > 0 && ingredient.item != null)
                        inventory.Remove(new Item(ingredient.item), ingredient.amount);

                // roll the dice to decide if we add the result or not
                // IMPORTANT: we use rand() < probability to decide.
                // => UnityEngine.Random.value is [0,1] inclusive:
                //    for 0% probability it's fine because it's never '< 0'
                //    for 100% probability it's not because it's not always '< 1', it might be == 1
                //    and if we use '<=' instead then it won't work for 0%
                // => C#'s Random value is [0,1) exclusive like most random
                //    functions. this works fine.
                if (new System.Random().NextDouble() < currentRecipe.probability)
                {
                    // add result item to inventory
                    inventory.Add(new Item(currentRecipe.result), 1);
                    TargetCraftingSuccess();
                }
                else
                {
                    TargetCraftingFailed();
                }

                // clear indices afterwards
                // note: we set all to -1 instead of calling .Clear because
                //       that would clear all the slots in host mode.
                // (don't clear in host mode, otherwise it clears the crafting
                //  UI for the player and we have to drag items into it again)
                if (!isLocalPlayer)
                    for (int i = 0; i < ScriptableRecipe.recipeSize; ++i)
                        indices[i] = -1;

                // clear recipe
                currentRecipe = null;
            }
        }
    }

    // two rpcs for results to save 1 byte for the actual result
    [TargetRpc] // only send to one client
    public void TargetCraftingSuccess()
    {
        state = CraftingState.Success;
    }

    [TargetRpc] // only send to one client
    public void TargetCraftingFailed()
    {
        state = CraftingState.Failed;
    }

    // drag & drop /////////////////////////////////////////////////////////////
    void OnDragAndDrop_InventorySlot_CraftingIngredientSlot(int[] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        // only if not crafting right now
        if (state != CraftingState.InProgress)
        {
            if (!indices.Contains(slotIndices[0]))
            {
                indices[slotIndices[1]] = slotIndices[0];
                state = CraftingState.None; // reset state
            }
        }
    }

    void OnDragAndDrop_CraftingIngredientSlot_CraftingIngredientSlot(int[] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        // only if not crafting right now
        if (state != CraftingState.InProgress)
        {
            // just swap them clientsided
            int temp = indices[slotIndices[0]];
            indices[slotIndices[0]] = indices[slotIndices[1]];
            indices[slotIndices[1]] = temp;
            state = CraftingState.None; // reset state
        }
    }

    void OnDragAndClear_CraftingIngredientSlot(int slotIndex)
    {
        // only if not crafting right now
        if (state != CraftingState.InProgress)
        {
            indices[slotIndex] = -1;
            state = CraftingState.None; // reset state
        }
    }
}
