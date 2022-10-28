using UnityEngine;
using UnityEngine.UI;

public partial class UIPortrait : MonoBehaviour
{
    public GameObject panel;
    public Image image;

    void Update()
    {
        PlayerOLD player = PlayerOLD.localPlayer;
        if (player)
        {
            panel.SetActive(true);
            image.sprite = player.portraitIcon;
        }
        else panel.SetActive(false);
    }
}
