// Note: this script has to be on an always-active UI parent, so that we can
// always find it from other code. (GameObject.Find doesn't find inactive ones)
using UnityEngine;
using UnityEngine.UI;

public partial class UIPlayerTrading : MonoBehaviour
{
    public GameObject panel;

    public UIPlayerTradingSlot slotPrefab;

    public Transform otherContent;
    public Text otherStatusText;
    public InputField otherGoldInput;

    public Transform myContent;
    public Text myStatusText;
    public InputField myGoldInput;

    public Button lockButton;
    public Button acceptButton;
    public Button cancelButton;

    [Header("Durability Colors")]
    public Color brokenDurabilityColor = Color.red;
    public Color lowDurabilityColor = Color.magenta;
    [Range(0.01f, 0.99f)] public float lowDurabilityThreshold = 0.1f;

    void Update()
    {
        Player player = Player.localPlayer;

        // only if trading, otherwise set inactive
        if (player != null &&
            player.state == "TRADING" &&
            player.target != null &&
            player.target is Player other)
        {
            panel.SetActive(true);

            // OTHER ///////////////////////////////////////////////////////////
            // status text
            if (other.trading.state == TradingState.Accepted) otherStatusText.text = "[ACCEPTED]";
            else if (other.trading.state == TradingState.Locked) otherStatusText.text = "[LOCKED]";
            else otherStatusText.text = "";

            // gold input
            otherGoldInput.text = other.trading.offerGold.ToString();

            // items
            UIUtils.BalancePrefabs(slotPrefab.gameObject, other.trading.offerItems.Count, otherContent);
            for (int i = 0; i < other.trading.offerItems.Count; ++i)
            {
                UIPlayerTradingSlot slot = otherContent.GetChild(i).GetComponent<UIPlayerTradingSlot>();
                int inventoryIndex = other.trading.offerItems[i];

                slot.dragAndDropable.dragable = false;
                slot.dragAndDropable.dropable = false;

                if (0 <= inventoryIndex && inventoryIndex < other.inventory.slots.Count &&
                    other.inventory.slots[inventoryIndex].amount > 0)
                {
                    ItemSlot itemSlot = other.inventory.slots[inventoryIndex];

                    // refresh valid item

                    // only build tooltip while it's actually shown. this
                    // avoids MASSIVE amounts of StringBuilder allocations.
                    slot.tooltip.enabled = true;
                    if (slot.tooltip.IsVisible())
                        slot.tooltip.text = itemSlot.ToolTip();

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
                    // refresh invalid item
                    slot.tooltip.enabled = false;
                    slot.image.color = Color.clear;
                    slot.image.sprite = null;
                    slot.amountOverlay.SetActive(false);
                }
            }

            // SELF ////////////////////////////////////////////////////////////
            // status text
            if (player.trading.state == TradingState.Accepted) myStatusText.text = "[ACCEPTED]";
            else if (player.trading.state == TradingState.Locked) myStatusText.text = "[LOCKED]";
            else myStatusText.text = "";

            // gold input
            if (player.trading.state == TradingState.Free)
            {
                myGoldInput.interactable = true;
                myGoldInput.onValueChanged.SetListener(val => {
                    long goldOffer = Utils.Clamp(val.ToLong(), 0, player.gold);
                    myGoldInput.text = goldOffer.ToString();
                    player.trading.CmdOfferGold(goldOffer);
                });
            }
            else
            {
                myGoldInput.interactable = false;
                myGoldInput.text = player.trading.offerGold.ToString();
            }

            // items
            UIUtils.BalancePrefabs(slotPrefab.gameObject, player.trading.offerItems.Count, myContent);
            for (int i = 0; i < player.trading.offerItems.Count; ++i)
            {
                UIPlayerTradingSlot slot = myContent.GetChild(i).GetComponent<UIPlayerTradingSlot>();
                slot.dragAndDropable.name = i.ToString(); // drag and drop index
                int inventoryIndex = player.trading.offerItems[i];

                if (0 <= inventoryIndex && inventoryIndex < player.inventory.slots.Count &&
                    player.inventory.slots[inventoryIndex].amount > 0)
                {
                    ItemSlot itemSlot = player.inventory.slots[inventoryIndex];

                    // refresh valid item

                    // only build tooltip while it's actually shown. this
                    // avoids MASSIVE amounts of StringBuilder allocations.
                    slot.tooltip.enabled = true;
                    if (slot.tooltip.IsVisible())
                        slot.tooltip.text = itemSlot.ToolTip();
                    slot.dragAndDropable.dragable = player.trading.state == TradingState.Free;

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
                    // refresh invalid item
                    slot.tooltip.enabled = false;
                    slot.dragAndDropable.dragable = false;
                    slot.image.color = Color.clear;
                    slot.image.sprite = null;
                    slot.amountOverlay.SetActive(false);
                }
            }

            // buttons /////////////////////////////////////////////////////////
            // lock
            lockButton.interactable = player.trading.state == TradingState.Free;
            lockButton.onClick.SetListener(() => {
                player.trading.CmdLockOffer();
            });

            // accept (only if both have locked the trade & if not accepted yet)
            // accept (if not accepted yet & other has locked or accepted)
            acceptButton.interactable = player.trading.state == TradingState.Locked &&
                                        other.trading.state != TradingState.Free;
            acceptButton.onClick.SetListener(() => {
                player.trading.CmdAcceptOffer();
            });

            // cancel
            cancelButton.onClick.SetListener(() => {
                player.trading.CmdCancel();
            });
        }
        else
        {
            panel.SetActive(false);
            myGoldInput.text = "0"; // reset
        }
    }
}
