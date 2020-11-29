// Note: this script has to be on an always-active UI parent, so that we can
// always find it from other code. (GameObject.Find doesn't find inactive ones)
using UnityEngine;
using UnityEngine.UI;

public partial class UIPlayerTradeRequest : MonoBehaviour
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
            // only if there is a request and if not accepted already
            if (player.trading.requestFrom != "" && player.state != "TRADING")
            {
                panel.SetActive(true);
                nameText.text = player.trading.requestFrom;
                acceptButton.onClick.SetListener(() => {
                    player.trading.CmdAcceptRequest();
                });
                declineButton.onClick.SetListener(() => {
                    player.trading.CmdDeclineRequest();
                });
            }
            else panel.SetActive(false);
        }
        else panel.SetActive(false);
    }
}
