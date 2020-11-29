// Note: this script has to be on an always-active UI parent, so that we can
// always find it from other code. (GameObject.Find doesn't find inactive ones)
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public partial class UILoot : MonoBehaviour
{
    public static UILoot singleton;
    public GameObject panel;
    public GameObject goldSlot;
    public Text goldText;
    public UILootSlot itemSlotPrefab;
    public Transform content;

    public UILoot()
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
            panel.activeSelf &&
            player.target != null &&
            player.target.health.current == 0 &&
            Utils.ClosestDistance(player, player.target) <= player.interactionRange &&
            player.target is Monster &&
            ((Monster)player.target).inventory.HasLoot())
        {
            // gold slot
            if (player.target.gold > 0)
            {
                goldSlot.SetActive(true);
                goldSlot.GetComponentInChildren<Button>().onClick.SetListener(() => {
                    player.looting.CmdTakeGold();
                });
                goldText.text = player.target.gold.ToString();
            }
            else goldSlot.SetActive(false);

            // IMPORTANT: when showing slots, we never filter out the empty ones.
            // the slot position should never change, otherwise party members
            // might accidentally click on the same slot at the same time, and
            // then the first person gets the item, the second person gets the
            // other item that he didn't click on because it was moved up.
            // => simply don't ever modify slot positions / indices!

            // instantiate/destroy enough slots
            // (we only want to show the non-empty slots)
            UIUtils.BalancePrefabs(itemSlotPrefab.gameObject, ((Monster)player.target).inventory.slots.Count, content);

            // refresh all valid items
            for (int i = 0; i < ((Monster)player.target).inventory.slots.Count; ++i)
            {
                ItemSlot itemSlot = ((Monster)player.target).inventory.slots[i];

                UILootSlot slot = content.GetChild(i).GetComponent<UILootSlot>();
                slot.dragAndDropable.name = i.ToString(); // drag and drop index

                if (itemSlot.amount > 0)
                {
                    // refresh valid item
                    slot.button.interactable = player.inventory.CanAdd(itemSlot.item, itemSlot.amount);
                    int icopy = i;
                    slot.button.onClick.SetListener(() => {
                        player.looting.CmdTakeItem(icopy);
                    });
                    // only build tooltip while it's actually shown. this
                    // avoids MASSIVE amounts of StringBuilder allocations.
                    slot.tooltip.enabled = true;
                    if (slot.tooltip.IsVisible())
                        slot.tooltip.text = itemSlot.ToolTip();
                    slot.image.color = Color.white;
                    slot.image.sprite = itemSlot.item.image;
                    slot.nameText.text = itemSlot.item.name;
                    slot.amountOverlay.SetActive(itemSlot.amount > 1);
                    slot.amountText.text = itemSlot.amount.ToString();
                }
                else
                {
                    // refresh invalid item
                    slot.button.interactable = false;
                    slot.button.onClick.RemoveAllListeners();
                    slot.tooltip.enabled = false;
                    slot.tooltip.text = "";
                    slot.image.color = Color.clear;
                    slot.image.sprite = null;
                    slot.nameText.text = "";
                    slot.amountOverlay.SetActive(false);
                }
            }
        }
        else panel.SetActive(false);
    }

    public void Show() { panel.SetActive(true); }
}
