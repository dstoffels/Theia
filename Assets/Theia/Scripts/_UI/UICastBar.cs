﻿using UnityEngine;
using UnityEngine.UI;

public partial class UICastBar : MonoBehaviour
{
    public GameObject panel;
    public Slider slider;
    public Text skillNameText;
    public Text progressText;

    void Update()
    {
        PlayerOLD player = PlayerOLD.localPlayer;
        if (player != null &&
            player.state == "CASTING" && player.skills.currentSkill != -1 &&
            player.skills.skills[player.skills.currentSkill].showCastBar)
        {
            panel.SetActive(true);

            SkillOLD skill = player.skills.skills[player.skills.currentSkill];
            float ratio = (skill.castTime - skill.CastTimeRemaining()) / skill.castTime;

            slider.value = ratio;
            skillNameText.text = skill.name;
            progressText.text = skill.CastTimeRemaining().ToString("F1") + "s";
        }
        else panel.SetActive(false);
    }
}
