// Strength Attribute that grants extra health.
// IMPORTANT: SyncMode=Observers needed to show other player's mana correctly!
using System;
using UnityEngine;

[DisallowMultipleComponent]
public class Intelligence : PlayerAttribute, IManaBonus
{
    // 1 point means 1% of max bonus
    public float manaBonusPercentPerPoint = 0.01f;

    public int GetManaBonus(int baseMana) =>
        Convert.ToInt32(baseMana * (value * manaBonusPercentPerPoint));

    public int GetManaRecoveryBonus() => 0;
}
