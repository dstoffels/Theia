using UnityEngine;
using Mirror;

[RequireComponent(typeof(PlayerChat))]
[RequireComponent(typeof(PlayerParty))]
public class PlayerExperience : Experience
{
    [Header("Components")]
    public PlayerChat chat;
    public PlayerParty party;

    [Header("Death")]
    public string deathMessage = "You died and lost experience.";

    [Server]
    public override void OnDeath()
    {
        // call base logic
        base.OnDeath();

        // send an info chat message
        chat.TargetMsgInfo(deathMessage);
    }

    // events //////////////////////////////////////////////////////////////////
    [Server]
    public void OnKilledEnemy(Entity victim)
    {
        // killed a monster
        if (victim is Monster monster)
        {
            // gain exp if not in a party or if in a party without exp share
            if (!party.InParty() || !party.party.shareExperience)
                current += BalanceExperienceReward(monster.rewardExperience, level.current, monster.level.current);
        }
    }
}
