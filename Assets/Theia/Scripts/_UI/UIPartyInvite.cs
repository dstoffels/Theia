// Note: this script has to be on an always-active UI parent, so that we can
// always find it from other code. (GameObject.Find doesn't find inactive ones)
using UnityEngine;
using UnityEngine.UI;

public partial class UIPartyInvite : MonoBehaviour
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
            if (player.party.inviteFrom != "")
            {
                panel.SetActive(true);
                nameText.text = player.party.inviteFrom;
                acceptButton.onClick.SetListener(() => {
                    player.party.CmdAcceptInvite();
                });
                declineButton.onClick.SetListener(() => {
                    player.party.CmdDeclineInvite();
                });
            }
            else panel.SetActive(false);
        }
        else panel.SetActive(false); // hide
    }
}
