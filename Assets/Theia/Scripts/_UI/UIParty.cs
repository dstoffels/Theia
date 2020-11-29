using UnityEngine;
using UnityEngine.UI;

public partial class UIParty : MonoBehaviour
{
    public KeyCode hotKey = KeyCode.P;
    public GameObject panel;
    public Text currentCapacityText;
    public Text maximumCapacityText;
    public UIPartyMemberSlot slotPrefab;
    public Transform memberContent;
    public Toggle experienceShareToggle;
    public Toggle goldShareToggle;

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
                Party party = player.party.party;
                int memberCount = party.members != null ? party.members.Length : 0;

                // properties
                currentCapacityText.text = memberCount.ToString();
                maximumCapacityText.text = Party.Capacity.ToString();

                // instantiate/destroy enough slots
                UIUtils.BalancePrefabs(slotPrefab.gameObject, memberCount, memberContent);

                // refresh all members
                for (int i = 0; i < memberCount; ++i)
                {
                    UIPartyMemberSlot slot = memberContent.GetChild(i).GetComponent<UIPartyMemberSlot>();
                    string memberName = party.members[i];

                    slot.nameText.text = memberName;
                    slot.masterIndicatorText.gameObject.SetActive(i == 0);

                    // party struct doesn't sync health, mana, level, etc. We find
                    // those from observers instead. Saves bandwidth and is good
                    // enough since another member's health is only really important
                    // to use when we are fighting the same monsters.
                    // => null if member not in observer range, in which case health
                    //    bars etc. should be grayed out!

                    // update some data only if around. otherwise keep previous data.
                    // update icon only if around. otherwise keep previous one.
                    if (Player.onlinePlayers.ContainsKey(memberName))
                    {
                        Player member = Player.onlinePlayers[memberName];
                        slot.icon.sprite = member.classIcon;
                        slot.levelText.text = member.level.current.ToString();
                        slot.guildText.text = member.guild.guild.name;
                        slot.healthSlider.value = member.health.Percent();
                        slot.manaSlider.value = member.mana.Percent();
                    }

                    // action button:
                    // dismiss: if i=0 and member=self and master
                    // kick: if i > 0 and player=master
                    // leave: if member=self and not master
                    if (memberName == player.name && i == 0)
                    {
                        slot.actionButton.gameObject.SetActive(true);
                        slot.actionButton.GetComponentInChildren<Text>().text = "Dismiss";
                        slot.actionButton.onClick.SetListener(() => {
                            player.party.CmdDismiss();
                        });
                    }
                    else if (memberName == player.name && i > 0)
                    {
                        slot.actionButton.gameObject.SetActive(true);
                        slot.actionButton.GetComponentInChildren<Text>().text = "Leave";
                        slot.actionButton.onClick.SetListener(() => {
                            player.party.CmdLeave();
                        });
                    }
                    else if (party.members[0] == player.name && i > 0)
                    {
                        slot.actionButton.gameObject.SetActive(true);
                        slot.actionButton.GetComponentInChildren<Text>().text = "Kick";
                        slot.actionButton.onClick.SetListener(() => {
                            player.party.CmdKick(memberName);
                        });
                    }
                    else
                    {
                        slot.actionButton.gameObject.SetActive(false);
                    }
                }

                // exp share toggle
                experienceShareToggle.interactable = player.party.InParty() && party.members[0] == player.name;
                experienceShareToggle.onValueChanged.SetListener((val) => {}); // avoid callback while setting .isOn via code
                experienceShareToggle.isOn = party.shareExperience;
                experienceShareToggle.onValueChanged.SetListener((val) => {
                    player.party.CmdSetExperienceShare(val);
                });

                // gold share toggle
                goldShareToggle.interactable = player.party.InParty() && party.members[0] == player.name;
                goldShareToggle.onValueChanged.SetListener((val) => {}); // avoid callback while setting .isOn via code
                goldShareToggle.isOn = party.shareGold;
                goldShareToggle.onValueChanged.SetListener((val) => {
                    player.party.CmdSetGoldShare(val);
                });
            }
        }
        else panel.SetActive(false);
    }
}
