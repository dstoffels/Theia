using UnityEngine;
using UnityEngine.UI;

public partial class UISkillbar : MonoBehaviour
{
    public GameObject panel;
    public UISkillbarSlot slotPrefab;
    public Transform content;

    [Header("Durability Colors")]
    public Color brokenDurabilityColor = Color.red;
    public Color lowDurabilityColor = Color.magenta;
    [Range(0.01f, 0.99f)] public float lowDurabilityThreshold = 0.1f;

    void Update()
    {
        Player player = Player.localPlayer;
        if (player != null)
        {
            panel.SetActive(true);

            // instantiate/destroy enough slots
            UIUtils.BalancePrefabs(slotPrefab.gameObject, player.skillbar.slots.Length, content);

            // refresh all
            for (int i = 0; i < player.skillbar.slots.Length; ++i)
            {
                SkillbarEntry entry = player.skillbar.slots[i];

                UISkillbarSlot slot = content.GetChild(i).GetComponent<UISkillbarSlot>();
                slot.dragAndDropable.name = i.ToString(); // drag and drop index

                // hotkey overlay (without 'Alpha' etc.)
                string pretty = entry.hotKey.ToString().Replace("Alpha", "");
                slot.hotkeyText.text = pretty;

                // skill, inventory item or equipment item?
                int skillIndex = player.skills.GetSkillIndexByName(entry.reference);
                int inventoryIndex = player.inventory.GetItemIndexByName(entry.reference);
                int equipmentIndex = player.equipment.GetItemIndexByName(entry.reference);
                if (skillIndex != -1)
                {
                    Skill skill = player.skills.skills[skillIndex];
                    bool canCast = player.skills.CastCheckSelf(skill);

                    // if movement does NOT support navigation then we need to
                    // check distance too. otherwise distance doesn't matter
                    // because we can navigate anywhere.
                    if (!player.movement.CanNavigate())
                        canCast &= player.skills.CastCheckDistance(skill, out Vector3 _);

                    // hotkey pressed and not typing in any input right now?
                    if (Input.GetKeyDown(entry.hotKey) &&
                        !UIUtils.AnyInputActive() &&
                        canCast) // checks mana, cooldowns, etc.) {
                    {
                        // try use the skill or walk closer if needed
                        ((PlayerSkills)player.skills).TryUse(skillIndex);
                    }

                    // refresh skill slot
                    slot.button.interactable = canCast; // check mana, cooldowns, etc.
                    slot.button.onClick.SetListener(() => {
                        // try use the skill or walk closer if needed
                        ((PlayerSkills)player.skills).TryUse(skillIndex);
                    });
                    // only build tooltip while it's actually shown. this
                    // avoids MASSIVE amounts of StringBuilder allocations.
                    slot.tooltip.enabled = true;
                    if (slot.tooltip.IsVisible())
                        slot.tooltip.text = skill.ToolTip();
                    slot.dragAndDropable.dragable = true;
                    slot.image.color = Color.white;
                    slot.image.sprite = skill.image;
                    float cooldown = skill.CooldownRemaining();
                    slot.cooldownOverlay.SetActive(cooldown > 0);
                    slot.cooldownText.text = cooldown.ToString("F0");
                    slot.cooldownCircle.fillAmount = skill.cooldown > 0 ? cooldown / skill.cooldown : 0;
                    slot.amountOverlay.SetActive(false);
                }
                else if (inventoryIndex != -1)
                {
                    ItemSlot itemSlot = player.inventory.slots[inventoryIndex];

                    // hotkey pressed and not typing in any input right now?
                    if (Input.GetKeyDown(entry.hotKey) && !UIUtils.AnyInputActive())
                        player.inventory.CmdUseItem(inventoryIndex);

                    // refresh inventory slot
                    slot.button.onClick.SetListener(() => {
                        player.inventory.CmdUseItem(inventoryIndex);
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

                    slot.cooldownOverlay.SetActive(false);
                    // cooldown if usable item
                    if (itemSlot.item.data is UsableItem usable)
                    {
                        float cooldown = player.GetItemCooldown(usable.cooldownCategory);
                        slot.cooldownCircle.fillAmount = usable.cooldown > 0 ? cooldown / usable.cooldown : 0;
                    }
                    else slot.cooldownCircle.fillAmount = 0;
                    slot.amountOverlay.SetActive(itemSlot.amount > 1);
                    slot.amountText.text = itemSlot.amount.ToString();
                }
                else if (equipmentIndex != -1)
                {
                    ItemSlot itemSlot = player.equipment.slots[equipmentIndex];

                    // refresh equipment slot
                    slot.button.onClick.RemoveAllListeners();
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

                    slot.cooldownOverlay.SetActive(false);
                    // cooldown if usable item
                    if (itemSlot.item.data is UsableItem usable)
                    {
                        float cooldown = player.GetItemCooldown(usable.cooldownCategory);
                        slot.cooldownCircle.fillAmount = usable.cooldown > 0 ? cooldown / usable.cooldown : 0;
                    }
                    else slot.cooldownCircle.fillAmount = 0;
                    slot.amountOverlay.SetActive(itemSlot.amount > 1);
                    slot.amountText.text = itemSlot.amount.ToString();
                }
                else
                {
                    // clear the outdated reference
                    // (need to assign directly because it's a struct)
                    player.skillbar.slots[i].reference = "";

                    // refresh empty slot
                    slot.button.onClick.RemoveAllListeners();
                    slot.tooltip.enabled = false;
                    slot.dragAndDropable.dragable = false;
                    slot.image.color = Color.clear;
                    slot.image.sprite = null;
                    slot.cooldownOverlay.SetActive(false);
                    slot.cooldownCircle.fillAmount = 0;
                    slot.amountOverlay.SetActive(false);
                }
            }
        }
        else panel.SetActive(false);
    }
}
