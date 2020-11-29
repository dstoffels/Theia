using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public partial class UICrafting : MonoBehaviour
{
    public KeyCode hotKey = KeyCode.C;
    public GameObject panel;
    public UICraftingIngredientSlot ingredientSlotPrefab;
    public Transform ingredientContent;
    public Image resultSlotImage;
    public UIShowToolTip resultSlotToolTip;
    public Button craftButton;
    public Slider progressSlider;
    public Text resultText;
    public Color successColor = Color.green;
    public Color failedColor = Color.red;

    [Header("Durability Colors")]
    public Color brokenDurabilityColor = Color.red;
    public Color lowDurabilityColor = Color.magenta;
    [Range(0.01f, 0.99f)] public float lowDurabilityThreshold = 0.1f;

    void Update()
    {
        Player player = Player.localPlayer;
        if (player)
        {
            // hotkey (not while typing in chat, etc.)
            if (Input.GetKeyDown(hotKey) && !UIUtils.AnyInputActive())
                panel.SetActive(!panel.activeSelf);

            // only update the panel if it's active
            if (panel.activeSelf)
            {
                // instantiate/destroy enough slots
                UIUtils.BalancePrefabs(ingredientSlotPrefab.gameObject, player.crafting.indices.Count, ingredientContent);

                // refresh all
                for (int i = 0; i < player.crafting.indices.Count; ++i)
                {
                    UICraftingIngredientSlot slot = ingredientContent.GetChild(i).GetComponent<UICraftingIngredientSlot>();
                    slot.dragAndDropable.name = i.ToString(); // drag and drop index
                    int itemIndex = player.crafting.indices[i];

                    if (0 <= itemIndex && itemIndex < player.inventory.slots.Count &&
                        player.inventory.slots[itemIndex].amount > 0)
                    {
                        ItemSlot itemSlot = player.inventory.slots[itemIndex];

                        // refresh valid item

                        // only build tooltip while it's actually shown. this
                        // avoids MASSIVE amounts of StringBuilder allocations.
                        slot.tooltip.enabled = true;
                        if (slot.tooltip.IsVisible())
                            slot.tooltip.text = itemSlot.ToolTip();
                        slot.dragAndDropable.dragable = true;

                        // use durability colors?
                        if (itemSlot.item.maxDurability > 0)
                        {
                            if (itemSlot.item.durability == 0)
                                slot.image.color = brokenDurabilityColor;
                            else if (itemSlot.item.DurabilityPercent() < lowDurabilityThreshold)
                                slot.image.color = lowDurabilityColor;
                            else
                                slot.image.color = Color.white;
                        }
                        else slot.image.color = Color.white; // reset for no-durability items
                        slot.image.sprite = itemSlot.item.image;

                        slot.amountOverlay.SetActive(itemSlot.amount > 1);
                        slot.amountText.text = itemSlot.amount.ToString();
                    }
                    else
                    {
                        // reset the index because it's invalid
                        player.crafting.indices[i] = -1;

                        // refresh invalid item
                        slot.tooltip.enabled = false;
                        slot.dragAndDropable.dragable = false;
                        slot.image.color = Color.clear;
                        slot.image.sprite = null;
                        slot.amountOverlay.SetActive(false);
                    }
                }

                // find valid indices => item templates => matching recipe
                List<int> validIndices = player.crafting.indices.Where(
                    index => 0 <= index && index < player.inventory.slots.Count &&
                             player.inventory.slots[index].amount > 0
                ).ToList();
                List<ItemSlot> items = validIndices.Select(index => player.inventory.slots[index]).ToList();
                ScriptableRecipe recipe = ScriptableRecipe.Find(items);
                if (recipe != null)
                {
                    // refresh valid recipe
                    Item item = new Item(recipe.result);
                    // only build tooltip while it's actually shown. this
                    // avoids MASSIVE amounts of StringBuilder allocations.
                    resultSlotToolTip.enabled = true;
                    if (resultSlotToolTip.IsVisible())
                        resultSlotToolTip.text = new ItemSlot(item).ToolTip(); // ItemSlot so that {AMOUNT} is replaced too
                    resultSlotImage.color = Color.white;
                    resultSlotImage.sprite = recipe.result.image;

                    // show progress bar while crafting
                    // (show 100% if craft time = 0 because it's just better feedback)
                    progressSlider.gameObject.SetActive(player.state == "CRAFTING");
                    double startTime = player.crafting.endTime - recipe.craftingTime;
                    double elapsedTime = NetworkTime.time - startTime;
                    progressSlider.value = recipe.craftingTime > 0 ? (float)elapsedTime / recipe.craftingTime : 1;
                }
                else
                {
                    // refresh invalid recipe
                    resultSlotToolTip.enabled = false;
                    resultSlotImage.color = Color.clear;
                    resultSlotImage.sprite = null;
                    progressSlider.gameObject.SetActive(false);
                }

                // craft result
                // (no recipe != null check because it will be null if those were
                //  the last two ingredients in our inventory)
                if (player.crafting.state == CraftingState.Success)
                {
                    resultText.color = successColor;
                    resultText.text = "Success!";
                }
                else if (player.crafting.state == CraftingState.Failed)
                {
                    resultText.color = failedColor;
                    resultText.text = "Failed :(";
                }
                else
                {
                    resultText.text = "";
                }

                // craft button with 'Try' prefix to let people know that it might fail
                // (disabled while in progress)
                craftButton.GetComponentInChildren<Text>().text = recipe != null &&
                                                                  recipe.probability < 1 ? "Try Craft" : "Craft";
                craftButton.interactable = recipe != null &&
                                           player.state != "CRAFTING" &&
                                           player.crafting.state!= CraftingState.InProgress &&
                                           player.inventory.CanAdd(new Item(recipe.result), 1);
                craftButton.onClick.SetListener(() => {
                    player.crafting.state = CraftingState.InProgress; // wait for result

                    // pass original array so server can copy it to it's own
                    // craftingIndices. we pass original one and not only the valid
                    // indicies because then in host mode we would make the crafting
                    // indices array smaller by only copying the valid indices,
                    // hence losing crafting slots
                    player.crafting.CmdCraft(recipe.name, player.crafting.indices.ToArray());
                });
            }
        }
        else panel.SetActive(false);
    }
}
