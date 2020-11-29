using UnityEngine;
using UnityEngine.UI;

public partial class UINpcTrading : MonoBehaviour
{
    public static UINpcTrading singleton;
    public GameObject panel;
    public UINpcTradingSlot slotPrefab;
    public Transform content;
    public UIDragAndDropable buySlot;
    public InputField buyAmountInput;
    public Text buyCostsText;
    public Button buyButton;
    public UIDragAndDropable sellSlot;
    public InputField sellAmountInput;
    public Text sellCostsText;
    public Button sellButton;
    public Button repairButton;
    [HideInInspector] public int buyIndex = -1;
    [HideInInspector] public int sellIndex = -1;

    [Header("Durability Colors")]
    public Color brokenDurabilityColor = Color.red;
    public Color lowDurabilityColor = Color.magenta;
    [Range(0.01f, 0.99f)] public float lowDurabilityThreshold = 0.1f;

    public UINpcTrading()
    {
        // assign singleton only once (to work with DontDestroyOnLoad when
        // using Zones / switching scenes)
        if (singleton == null) singleton = this;
    }

    void Update()
    {
        Player player = Player.localPlayer;

        // use collider point(s) to also work with big entities
        if (player != null &&
            player.target != null &&
            player.target is Npc npc &&
            Utils.ClosestDistance(player, player.target) <= player.interactionRange)
        {
            // items for sale
            UIUtils.BalancePrefabs(slotPrefab.gameObject, npc.trading.saleItems.Length, content);
            for (int i = 0; i < npc.trading.saleItems.Length; ++i)
            {
                UINpcTradingSlot slot = content.GetChild(i).GetComponent<UINpcTradingSlot>();
                ScriptableItem itemData = npc.trading.saleItems[i];

                // show item in UI
                int icopy = i;
                slot.button.onClick.SetListener(() => {
                    buyIndex = icopy;
                });
                slot.image.color = Color.white;
                slot.image.sprite = itemData.image;
                // only build tooltip while it's actually shown. this
                // avoids MASSIVE amounts of StringBuilder allocations.
                slot.tooltip.enabled = true;
                if (slot.tooltip.IsVisible())
                    slot.tooltip.text = new ItemSlot(new Item(itemData)).ToolTip(); // with slot for {AMOUNT}
            }

            // buy
            if (buyIndex != -1 && buyIndex < npc.trading.saleItems.Length)
            {
                ScriptableItem itemData = npc.trading.saleItems[buyIndex];

                // make valid amount, calculate price
                int amount = buyAmountInput.text.ToInt();
                amount = Mathf.Clamp(amount, 1, itemData.maxStack);
                long price = amount * itemData.buyPrice;

                // show buy panel with item in UI
                buyAmountInput.text = amount.ToString();
                buySlot.GetComponent<Image>().color = Color.white;
                buySlot.GetComponent<Image>().sprite = itemData.image;
                // only build tooltip while it's actually shown. this
                // avoids MASSIVE amounts of StringBuilder allocations.
                buySlot.GetComponent<UIShowToolTip>().enabled = true;
                if (buySlot.GetComponent<UIShowToolTip>().IsVisible())
                    buySlot.GetComponent<UIShowToolTip>().text = new ItemSlot(new Item(itemData)).ToolTip(); // with slot for {AMOUNT}
                buySlot.dragable = true;
                buyCostsText.text = price.ToString();
                buyButton.interactable = amount > 0 && price <= player.gold &&
                                         player.inventory.CanAdd(new Item(itemData), amount);
                buyButton.onClick.SetListener(() => {
                    player.npcTrading.CmdBuyItem(buyIndex, amount);
                    buyIndex = -1;
                    buyAmountInput.text = "1";
                });
            }
            else
            {
                // show default buy panel in UI
                buySlot.GetComponent<Image>().color = Color.clear;
                buySlot.GetComponent<Image>().sprite = null;
                buySlot.GetComponent<UIShowToolTip>().enabled = false;
                buySlot.dragable = false;
                buyCostsText.text = "0";
                buyButton.interactable = false;
            }

            // sell
            if (sellIndex != -1 && sellIndex < player.inventory.slots.Count &&
                player.inventory.slots[sellIndex].amount > 0)
            {
                ItemSlot itemSlot = player.inventory.slots[sellIndex];

                // make valid amount, calculate price
                int amount = sellAmountInput.text.ToInt();
                amount = Mathf.Clamp(amount, 1, itemSlot.amount);
                long price = amount * itemSlot.item.sellPrice;

                // show sell panel with item in UI
                sellAmountInput.text = amount.ToString();

                // use durability colors?
                if (itemSlot.item.maxDurability > 0)
                {
                    if (itemSlot.item.durability == 0)
                        sellSlot.GetComponent<Image>().color = brokenDurabilityColor;
                    else if (itemSlot.item.DurabilityPercent() < lowDurabilityThreshold)
                        sellSlot.GetComponent<Image>().color = lowDurabilityColor;
                    else
                        sellSlot.GetComponent<Image>().color = Color.white;
                }
                else sellSlot.GetComponent<Image>().color = Color.white; // reset for no-durability items
                sellSlot.GetComponent<Image>().sprite = itemSlot.item.image;
                // only build tooltip while it's actually shown. this
                // avoids MASSIVE amounts of StringBuilder allocations.
                sellSlot.GetComponent<UIShowToolTip>().enabled = true;
                if (sellSlot.GetComponent<UIShowToolTip>().IsVisible())
                    sellSlot.GetComponent<UIShowToolTip>().text = itemSlot.ToolTip();
                sellSlot.dragable = true;
                sellCostsText.text = price.ToString();
                sellButton.interactable = amount > 0;
                sellButton.onClick.SetListener(() => {
                    player.npcTrading.CmdSellItem(sellIndex, amount);
                    sellIndex = -1;
                    sellAmountInput.text = "1";
                });
            }
            else
            {
                // show default sell panel in UI
                sellSlot.GetComponent<Image>().color = Color.clear;
                sellSlot.GetComponent<Image>().sprite = null;
                sellSlot.GetComponent<UIShowToolTip>().enabled = false;
                sellSlot.dragable = false;
                sellCostsText.text = "0";
                sellButton.interactable = false;
            }

            // repair items
            if (npc.trading.offersRepair)
            {
                int missing = player.inventory.GetTotalMissingDurability() +
                              player.equipment.GetTotalMissingDurability();
                int price = missing * npc.trading.repairCostPerDurabilityPoint;

                repairButton.gameObject.SetActive(true);
                repairButton.interactable = player.gold >= price;
                repairButton.onClick.SetListener(() => {
                    UIConfirmation.singleton.Show("Repair all Items for: " + price + " gold?", () => {
                        player.npcTrading.CmdRepairAllItems();
                    });
                });
            }
            else
            {
                repairButton.gameObject.SetActive(false);
            }
        }
        else panel.SetActive(false);
    }
}
