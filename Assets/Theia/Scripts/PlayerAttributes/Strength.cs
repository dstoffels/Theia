// Strength Attribute that grants extra health.
// IMPORTANT: SyncMode=Observers needed to show other player's health correctly!
using System;
using UnityEngine;

[DisallowMultipleComponent]
public class Strength : PlayerAttribute, IHealthBonus
{
    // 1 point means 1% of max bonus
    public float healthBonusPercentPerPoint = 0.01f;

    public int GetHealthBonus(int baseHealth) =>
        Convert.ToInt32(baseHealth * (value * healthBonusPercentPerPoint));

    public int GetHealthRecoveryBonus() => 0;
}
