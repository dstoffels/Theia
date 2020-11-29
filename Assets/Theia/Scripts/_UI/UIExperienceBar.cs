using UnityEngine;
using UnityEngine.UI;

public partial class UIExperienceBar : MonoBehaviour
{
    public GameObject panel;
    public Slider slider;
    public Text statusText;

    void Update()
    {
        Player player = Player.localPlayer;
        if (player)
        {
            panel.SetActive(true);
            slider.value = player.experience.Percent();
            statusText.text = "Lv." + player.level.current + " (" + (player.experience.Percent() * 100).ToString("F2") + "%)";
        }
        else panel.SetActive(false);
    }
}
