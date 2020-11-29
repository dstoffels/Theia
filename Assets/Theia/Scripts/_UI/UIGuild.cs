using UnityEngine;
using UnityEngine.UI;
using Mirror;

public partial class UIGuild : MonoBehaviour
{
    public KeyCode hotKey = KeyCode.G;
    public GameObject panel;
    public Text nameText;
    public Text masterText;
    public Text currentCapacityText;
    public Text maximumCapacityText;
    public InputField noticeInput;
    public Button noticeEditButton;
    public Button noticeSetButton;
    public UIGuildMemberSlot slotPrefab;
    public Transform memberContent;
    public Color onlineColor = Color.cyan;
    public Color offlineColor = Color.gray;
    public Button leaveButton;

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
                Guild currentGuild = player.guild.guild;
                int memberCount = currentGuild.members != null ? currentGuild.members.Length : 0;

                // guild properties
                nameText.text = player.guild.guild.name;
                masterText.text = currentGuild.master;
                currentCapacityText.text = memberCount.ToString();
                maximumCapacityText.text = GuildSystem.Capacity.ToString();

                // notice edit button
                noticeEditButton.interactable = currentGuild.CanNotify(player.name) &&
                                                !noticeInput.interactable;
                noticeEditButton.onClick.SetListener(() => {
                    noticeInput.interactable = true;
                });

                // notice set button
                noticeSetButton.interactable = currentGuild.CanNotify(player.name) &&
                                               noticeInput.interactable &&
                                               NetworkTime.time >= player.nextRiskyActionTime;
                noticeSetButton.onClick.SetListener(() => {
                    noticeInput.interactable = false;
                    if (noticeInput.text.Length > 0 &&
                        !string.IsNullOrWhiteSpace(noticeInput.text) &&
                        noticeInput.text != currentGuild.notice) {
                        player.guild.CmdSetNotice(noticeInput.text);
                    }
                });

                // notice input: copies notice while not editing it
                if (!noticeInput.interactable) noticeInput.text = currentGuild.notice ?? "";
                noticeInput.characterLimit = GuildSystem.NoticeMaxLength;

                // leave
                leaveButton.interactable = currentGuild.CanLeave(player.name);
                leaveButton.onClick.SetListener(() => {
                    player.guild.CmdLeave();
                });

                // instantiate/destroy enough slots
                UIUtils.BalancePrefabs(slotPrefab.gameObject, memberCount, memberContent);

                // refresh all members
                for (int i = 0; i < memberCount; ++i)
                {
                    UIGuildMemberSlot slot = memberContent.GetChild(i).GetComponent<UIGuildMemberSlot>();
                    GuildMember member = currentGuild.members[i];

                    slot.onlineStatusImage.color = member.online ? onlineColor : offlineColor;
                    slot.nameText.text = member.name;
                    slot.levelText.text = member.level.ToString();
                    slot.rankText.text = member.rank.ToString();
                    slot.promoteButton.interactable = currentGuild.CanPromote(player.name, member.name);
                    slot.promoteButton.onClick.SetListener(() => {
                        player.guild.CmdPromote(member.name);
                    });
                    slot.demoteButton.interactable = currentGuild.CanDemote(player.name, member.name);
                    slot.demoteButton.onClick.SetListener(() => {
                        player.guild.CmdDemote(member.name);
                    });
                    slot.kickButton.interactable = currentGuild.CanKick(player.name, member.name);
                    slot.kickButton.onClick.SetListener(() => {
                        player.guild.CmdKick(member.name);
                    });
                }
            }
        }
        else panel.SetActive(false);
    }
}
