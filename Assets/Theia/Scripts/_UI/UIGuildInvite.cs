// Note: this script has to be on an always-active UI parent, so that we can
// always find it from other code. (GameObject.Find doesn't find inactive ones)
using UnityEngine;
using UnityEngine.UI;

public partial class UIGuildInvite : MonoBehaviour
{
    public GameObject panel;
    public Text nameText;
    public Button acceptButton;
    public Button declineButton;

    void Update()
    {
        Player player = Player.localPlayer;
        if (player != null)
        {
            if (player.guild.inviteFrom != "")
            {
                panel.SetActive(true);
                nameText.text = player.guild.inviteFrom;
                acceptButton.onClick.SetListener(() => {
                    player.guild.CmdInviteAccept();
                });
                declineButton.onClick.SetListener(() => {
                    player.guild.CmdInviteDecline();
                });
            }
            else panel.SetActive(false);
        }
        else panel.SetActive(false);
    }
}
