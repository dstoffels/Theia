using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[DisallowMultipleComponent]
public class PlayerParty : NetworkBehaviour
{
    [Header("Components")]
    public PlayerOLD player;

    // .party is a copy for easier reading/syncing. Use PartySystem to manage
    // parties!
    [Header("Party")]
    [SyncVar, HideInInspector] public Party party; // TODO SyncToOwner later
    [SyncVar, HideInInspector] public string inviteFrom = "";
    public float inviteWaitSeconds = 3;

    // cache: only create List once, avoid allocating in each GetInProximity call
    List<PlayerOLD> proximity = new List<PlayerOLD>();

    void OnDestroy()
    {
        // do nothing if not spawned (=for character selection previews)
        if (!isServer && !isClient) return;

        if (isServer)
        {
            // leave party (if any)
            if (InParty())
            {
                // dismiss if master, leave otherwise
                if (party.master == name)
                    Dismiss();
                else
                    Leave();
            }
        }
    }

    // party ///////////////////////////////////////////////////////////////////
    public bool InParty()
    {
        // 0 means no party, because default party struct's partyId is 0.
        return party.partyId > 0;
    }

    // find party members in proximity for item/exp sharing etc.
    public List<PlayerOLD> GetMembersInProximity()
    {
        // clear proximity cache instead of allocating a new one each time
        proximity.Clear();

        if (InParty())
        {
            // (avoid Linq because it is HEAVY(!) on GC and performance)
            foreach (NetworkConnection conn in netIdentity.observers.Values)
            {
                PlayerOLD observer = conn.identity.GetComponent<PlayerOLD>();
                if (party.Contains(observer.name))
                    proximity.Add(observer);
            }
        }
        return proximity;
    }

    // party invite by name (not by target) so that chat commands are possible
    // if needed
    [Command]
    public void CmdInvite(string otherName)
    {
        // validate: is there someone with that name, and not self?
        if (otherName != name &&
            PlayerOLD.onlinePlayers.TryGetValue(otherName, out PlayerOLD other) &&
            NetworkTime.time >= player.nextRiskyActionTime)
        {
            // can only send invite if no party yet or party isn't full and
            // have invite rights and other guy isn't in party yet
            if ((!InParty() || !party.IsFull()) && !other.party.InParty())
            {
                // send an invite
                other.party.inviteFrom = name;
                Debug.Log(name + " invited " + other.name + " to party");
            }
        }

        // reset risky time no matter what. even if invite failed, we don't want
        // players to be able to spam the invite button and mass invite random
        // players.
        player.nextRiskyActionTime = NetworkTime.time + inviteWaitSeconds;
    }

    [Command]
    public void CmdAcceptInvite()
    {
        // valid invitation?
        // note: no distance check because sender might be far away already
        if (!InParty() && inviteFrom != "" &&
            PlayerOLD.onlinePlayers.TryGetValue(inviteFrom, out PlayerOLD sender))
        {
            // is in party? then try to add
            if (sender.party.InParty())
                PartySystem.AddToParty(sender.party.party.partyId, name);
            // otherwise try to form a new one
            else
                PartySystem.FormParty(sender.name, name);
        }

        // reset party invite in any case
        inviteFrom = "";
    }

    [Command]
    public void CmdDeclineInvite()
    {
        inviteFrom = "";
    }

    [Command]
    public void CmdKick(string member)
    {
        // try to kick. party system will do all the validation.
        PartySystem.KickFromParty(party.partyId, name, member);
    }

    // version without cmd because we need to call it from the server too
    public void Leave()
    {
        // try to leave. party system will do all the validation.
        PartySystem.LeaveParty(party.partyId, name);
    }
    [Command]
    public void CmdLeave() { Leave(); }

    // version without cmd because we need to call it from the server too
    public void Dismiss()
    {
        // try to dismiss. party system will do all the validation.
        PartySystem.DismissParty(party.partyId, name);
    }
    [Command]
    public void CmdDismiss() { Dismiss(); }

    [Command]
    public void CmdSetExperienceShare(bool value)
    {
        // try to set. party system will do all the validation.
        PartySystem.SetPartyExperienceShare(party.partyId, name, value);
    }

    [Command]
    public void CmdSetGoldShare(bool value)
    {
        // try to set. party system will do all the validation.
        PartySystem.SetPartyGoldShare(party.partyId, name, value);
    }

    // helper function to calculate the experience rewards for sharing parties
    public static long CalculateExperienceShare(long total, int memberCount, float bonusPercentagePerMember, int memberLevel, int killedLevel)
    {
        // bonus percentage based on how many members there are
        float bonusPercentage = (memberCount-1) * bonusPercentagePerMember;

        // calculate the share via ceil, so that uneven numbers still result in
        // at least 'total' in the end.
        // e.g. 4/2=2 (good); 5/2=2 (1 point got lost)
        long share = (long)Mathf.Ceil(total / (float)memberCount);

        // balance experience reward for the receiver's level. this is important
        // to avoid crazy power leveling where a level 1 hero would get a LOT of
        // level ups if his friend kills a level 100 monster once.
        long balanced = Experience.BalanceExperienceReward(share, memberLevel, killedLevel);
        long bonus = Convert.ToInt64(balanced * bonusPercentage);

        return balanced + bonus;
    }

    // combat //////////////////////////////////////////////////////////////////
    [Server]
    public void OnKilledEnemy(EntityOLD victim)
    {
        // currently in a party?
        if (InParty())
        {
            List<PlayerOLD> closeMembers = GetMembersInProximity();

            // forward kill to each member's quest
            // (not for us since PlayerQuests already takes care of that)
            foreach (PlayerOLD member in closeMembers)
                if (member != player)
                    member.quests.OnKilledEnemy(victim);

            // share experience & skill experience if monster
            // note: bonus only applies to exp. share parties, victimwise
            //       there's an unnecessary pressure to always join a
            //       party when leveling alone too.
            // note: if monster.rewardExp is 10 then it's possible that
            //       two members only receive 2 exp each (= 4 total).
            //       this happens because of exp balancing by level and
            //       is as intended.
            if (victim is Monster monster && party.shareExperience)
            {
                foreach (PlayerOLD member in closeMembers)
                {
                    member.experience.current += CalculateExperienceShare(
                        monster.rewardExperience,
                        closeMembers.Count,
                        Party.BonusExperiencePerMember,
                        member.level.current,
                        victim.level.current
                    );
                    ((PlayerSkills)member.skills).skillExperience += CalculateExperienceShare(
                        monster.rewardSkillExperience,
                        closeMembers.Count,
                        Party.BonusExperiencePerMember,
                        member.level.current,
                        victim.level.current
                    );
                }
            }
        }
    }
}
