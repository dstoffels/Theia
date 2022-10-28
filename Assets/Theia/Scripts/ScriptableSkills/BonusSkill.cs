// Base type for bonus skill templates.
// => can be used for passive skills, buffs, etc.
using System.Text;
using UnityEngine;

public abstract class BonusSkill : ScriptableSkill
{
    public LinearInt healthMaxBonus;
    public LinearInt manaMaxBonus;
    public LinearInt damageBonus;
    public LinearInt defenseBonus;
    public LinearFloat blockChanceBonus; // range [0,1]
    public LinearFloat criticalChanceBonus; // range [0,1]
    public LinearFloat healthPercentPerSecondBonus; // 0.1=10%; can be negative too
    public LinearFloat manaPercentPerSecondBonus; // 0.1=10%; can be negative too
    public LinearFloat speedBonus; // can be negative too

    // tooltip
    public override string ToolTip(int skillLevel, bool showRequirements = false)
    {
        StringBuilder tip = new StringBuilder(base.ToolTip(skillLevel, showRequirements));
        tip.Replace("{HEALTHMAXBONUS}", healthMaxBonus.Get(skillLevel).ToString());
        tip.Replace("{MANAMAXBONUS}", manaMaxBonus.Get(skillLevel).ToString());
        tip.Replace("{DAMAGEBONUS}", damageBonus.Get(skillLevel).ToString());
        tip.Replace("{DEFENSEBONUS}", defenseBonus.Get(skillLevel).ToString());
        tip.Replace("{BLOCKCHANCEBONUS}", Mathf.RoundToInt(blockChanceBonus.Get(skillLevel) * 100).ToString());
        tip.Replace("{CRITICALCHANCEBONUS}", Mathf.RoundToInt(criticalChanceBonus.Get(skillLevel) * 100).ToString());
        tip.Replace("{HEALTHPERCENTPERSECONDBONUS}", Mathf.RoundToInt(healthPercentPerSecondBonus.Get(skillLevel) * 100).ToString());
        tip.Replace("{MANAPERCENTPERSECONDBONUS}", Mathf.RoundToInt(manaPercentPerSecondBonus.Get(skillLevel) * 100).ToString());
        tip.Replace("{SPEEDBONUS}", speedBonus.Get(skillLevel).ToString("F2"));
        return tip.ToString();
    }
}
