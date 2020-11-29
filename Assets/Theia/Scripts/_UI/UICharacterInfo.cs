// Note: this script has to be on an always-active UI parent, so that we can
// always react to the hotkey.
using UnityEngine;
using UnityEngine.UI;

public partial class UICharacterInfo : MonoBehaviour
{
    public KeyCode hotKey = KeyCode.T;
    public GameObject panel;
    public Text damageText;
    public Text defenseText;
    public Text healthText;
    public Text manaText;
    public Text criticalChanceText;
    public Text blockChanceText;
    public Text speedText;
    public Text levelText;
    public Text currentExperienceText;
    public Text maximumExperienceText;
    public Text skillExperienceText;
    public Text attributesText;
    public Text strengthText;
    public Text intelligenceText;
    public Button strengthButton;
    public Button intelligenceButton;

    // remember default attributes header text so we can append "(remaining)"
    string attributesTextDefault;

    void Awake()
    {
        attributesTextDefault = attributesText.text;
    }

    void Update()
    {
        Player player = Player.localPlayer;
        if (player)
        {
            // hotkey (not while typing in chat, etc.)
            if (Input.GetKeyDown(hotKey) && !UIUtils.AnyInputActive())
                panel.SetActive(!panel.activeSelf);

            // only refresh the panel while it's active
            if (panel.activeSelf)
            {
                damageText.text = player.combat.damage.ToString();
                defenseText.text = player.combat.defense.ToString();
                healthText.text = player.health.max.ToString();
                manaText.text = player.mana.max.ToString();
                criticalChanceText.text = (player.combat.criticalChance * 100).ToString("F0") + "%";
                blockChanceText.text = (player.combat.blockChance * 100).ToString("F0") + "%";
                speedText.text = player.speed.ToString("F1");
                levelText.text = player.level.current.ToString();
                currentExperienceText.text = player.experience.current.ToString();
                maximumExperienceText.text = player.experience.max.ToString();
                skillExperienceText.text = ((PlayerSkills)player.skills).skillExperience.ToString();

                // attributes (show spendable if >1 so it's more obvious)
                // (each Attribute component has .PointsSpendable. can use any.)
                int spendable = player.strength.PointsSpendable();
                string suffix = "";
                if (spendable > 0)
                    suffix = " (" + player.strength.PointsSpendable() + ")";
                attributesText.text = attributesTextDefault + suffix;

                strengthText.text = player.strength.value.ToString();
                strengthButton.interactable = player.strength.PointsSpendable() > 0;
                strengthButton.onClick.SetListener(() => {
                    player.strength.CmdIncrease();
                });

                intelligenceText.text = player.intelligence.value.ToString();
                intelligenceButton.interactable = player.intelligence.PointsSpendable() > 0;
                intelligenceButton.onClick.SetListener(() => {
                    player.intelligence.CmdIncrease();
                });
            }
        }
        else panel.SetActive(false);
    }
}
