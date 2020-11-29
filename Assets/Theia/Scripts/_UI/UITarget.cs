// Note: this script has to be on an always-active UI parent, so that we can
// always find it from other code. (GameObject.Find doesn't find inactive ones)
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public partial class UITarget : MonoBehaviour
{
    public GameObject panel;
    public Slider healthSlider;
    public Text nameText;
    public Transform buffsPanel;
    public UIBuffSlot buffSlotPrefab;
    public Button tradeButton;
    public Button guildInviteButton;
    public Button partyInviteButton;

    void Update()
    {
        Player player = Player.localPlayer;
        if (player != null)
        {
            // show nextTarget > target
            // => feels best in situations where we select another target while
            //    casting a skill on the existing target.
            // => '.target' doesn't change while casting, but the UI gives the
            //    illusion that we already targeted something else
            // => this is also great for skills that change the target while casting,
            //    e.g. a buff that is cast on 'self' even though we target an 'npc.
            //    this way the player doesn't see the target switching.
            // => this is how most MMORPGs do it too.
            Entity target = player.nextTarget ?? player.target;
            if (target != null && target != player)
            {
                float distance = Utils.ClosestDistance(player, target);

                // name and health
                panel.SetActive(true);
                healthSlider.value = target.health.Percent();
                nameText.text = target.name;

                // target buffs
                UIUtils.BalancePrefabs(buffSlotPrefab.gameObject, target.skills.buffs.Count, buffsPanel);
                for (int i = 0; i < target.skills.buffs.Count; ++i)
                {
                    Buff buff = target.skills.buffs[i];
                    UIBuffSlot slot = buffsPanel.GetChild(i).GetComponent<UIBuffSlot>();

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

                // trade button
                if (target is Player)
                {
                    tradeButton.gameObject.SetActive(true);
                    tradeButton.interactable = player.trading.CanStartTradeWith(target);
                    tradeButton.onClick.SetListener(() => {
                        player.trading.CmdSendRequest();
                    });
                }
                else tradeButton.gameObject.SetActive(false);

                // guild invite button
                if (target is Player targetPlayer && player.guild.InGuild())
                {
                    guildInviteButton.gameObject.SetActive(true);
                    guildInviteButton.interactable = !targetPlayer.guild.InGuild() &&
                                                     player.guild.guild.CanInvite(player.name, target.name) &&
                                                     NetworkTime.time >= player.nextRiskyActionTime &&
                                                     distance <= player.interactionRange;
                    guildInviteButton.onClick.SetListener(() => {
                        player.guild.CmdInviteTarget();
                    });
                }
                else guildInviteButton.gameObject.SetActive(false);

                // party invite button
                if (target is Player targetPlayer2)
                {
                    partyInviteButton.gameObject.SetActive(true);
                    partyInviteButton.interactable = (!player.party.InParty() || !player.party.party.IsFull()) &&
                                                     !targetPlayer2.party.InParty() &&
                                                     NetworkTime.time >= player.nextRiskyActionTime &&
                                                     distance <= player.interactionRange;
                    partyInviteButton.onClick.SetListener(() => {
                        player.party.CmdInvite(target.name);
                    });
                }
                else partyInviteButton.gameObject.SetActive(false);
            }
            else panel.SetActive(false);
        }
        else panel.SetActive(false);
    }
}
