using UnityEngine;
using UnityEngine.UI;

public partial class UIBuffs : MonoBehaviour
{
    public GameObject panel;
    public UIBuffSlot slotPrefab;

    void Update()
    {
        PlayerOLD player = PlayerOLD.localPlayer;
        if (player)
        {
            panel.SetActive(true);

            // instantiate/destroy enough slots
            UIUtils.BalancePrefabs(slotPrefab.gameObject, player.skillsOLD.buffs.Count, panel.transform);

            // refresh all
            for (int i = 0; i < player.skillsOLD.buffs.Count; ++i)
            {
                Buff buff = player.skillsOLD.buffs[i];
                UIBuffSlot slot = panel.transform.GetChild(i).GetComponent<UIBuffSlot>();

                // refresh
                slot.image.color = Color.white;
                slot.image.sprite = buff.image;
                // only build tooltip while it's actually shown. this
                // avoids MASSIVE amounts of StringBuilder allocations.
                if (slot.tooltip.IsVisible())
                    slot.tooltip.text = buff.ToolTip();
                slot.slider.maxValue = buff.buffTime;
                slot.slider.value = buff.BuffTimeRemaining();
            }
        }
        else panel.SetActive(false);
    }
}