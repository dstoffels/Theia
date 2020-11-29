// Note: this script has to be on an always-active UI parent, so that we can
// always react to the hotkey.
using UnityEngine;
using UnityEngine.UI;

public partial class UIInventory : MonoBehaviour
{
    public static UIInventory singleton;
    public KeyCode hotKey = KeyCode.I;
    public GameObject panel;
    public UIInventorySlot slotPrefab;
    public Transform content;
    public Text goldText;
    public UIDragAndDropable trash;
    public Image trashImage;
    public GameObject trashOverlay;
    public Text trashAmountText;

    [Header("Durability Colors")]
    public Color brokenDurabilityColor = Color.red;
    public Color lowDurabilityColor = Color.magenta;
    [Range(0.01f, 0.99f)] public float lowDurabilityThreshold = 0.1f;

    public UIInventory()
    {
        // assign singleton only once (to work with DontDestroyOnLoad when
        // using Zones / switching scenes)
        if (singleton == null) singleton = this;
    }

    void Update()
    {
        Player player = Player.localPlayer;
        if (player != null)
        {
            // hotkey (not while typing in chat, etc.)
            if (Input.GetKeyDown(hotKey) && !UIUtils.AnyInputActive())
                panel.SetActive(!panel.activeSelf);

            // only update the panel if it's active
            if (panel.activeSelf)
            {
                // instantiate/destroy enough slots
                UIUtils.BalancePrefabs(slotPrefab.gameObject, player.inventory.slots.Count, content);

                // refresh all items
                for (int i = 0; i < player.inventory.slots.Count; ++i)
                {
                    UIInventorySlot slot = content.GetChild(i).GetComponent<UIInventorySlot>();
                    slot.dragAndDropable.name = i.ToString(); // drag and drop index
                    ItemSlot itemSlot = player.inventory.slots[i];

                    if (itemSlot.amount > 0)
                    {
                        // refresh valid item
                        int icopy = i; // needed for lambdas, otherwise i is Count
                        slot.button.onClick.SetListener(() => {
                            if (itemSlot.item.data is UsableItem usable &&
                                usable.CanUse(player, icopy))
                                player.inventory.CmdUseItem(icopy);
                        });
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

                        // cooldown if usable item
                        if (itemSlot.item.data is UsableItem usable2)
                        {
                            float cooldown = player.GetItemCooldown(usable2.cooldownCategory);
                            slot.cooldownCircle.fillAmount = usable2.cooldown > 0 ? cooldown / usable2.cooldown : 0;
                        }
                        else slot.cooldownCircle.fillAmount = 0;
                        slot.amountOverlay.SetActive(itemSlot.amount > 1);
                        slot.amountText.text = itemSlot.amount.ToString();
                    }
                    else
                    {
                        // refresh invalid item
                        slot.button.onClick.RemoveAllListeners();
                        slot.tooltip.enabled = false;
                        slot.dragAndDropable.dragable = false;
                        slot.image.color = Color.clear;
                        slot.image.sprite = null;
                        slot.cooldownCircle.fillAmount = 0;
                        slot.amountOverlay.SetActive(false);
                    }
                }

                // gold
                goldText.text = player.gold.ToString();

                // trash (tooltip always enabled, dropable always true)
                trash.dragable = player.inventory.trash.amount > 0;
                if (player.inventory.trash.amount > 0)
                {
                    // refresh valid item
                    if (player.inventory.trash.item.maxDurability > 0)
                    {
                        if (player.inventory.trash.item.durability == 0)
                            trashImage.color = brokenDurabilityColor;
                        else if (player.inventory.trash.item.DurabilityPercent() < lowDurabilityThreshold)
                            trashImage.color = lowDurabilityColor;
                        else
                            trashImage.color = Color.white;
                    }
                    else trashImage.color = Color.white; // reset for no-durability items
                    trashImage.sprite = player.inventory.trash.item.image;

                    trashOverlay.SetActive(player.inventory.trash.amount > 1);
                    trashAmountText.text = player.inventory.trash.amount.ToString();
                }
                else
                {
                    // refresh invalid item
                    trashImage.color = Color.clear;
                    trashImage.sprite = null;
                    trashOverlay.SetActive(false);
                }
            }
        }
        else panel.SetActive(false);
    }
}
