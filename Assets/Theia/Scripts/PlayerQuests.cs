using UnityEngine;
using Mirror;

[RequireComponent(typeof(PlayerInventory))]
[DisallowMultipleComponent]
public class PlayerQuests : NetworkBehaviour
{
    [Header("Components")]
    public PlayerOLD player;
    public PlayerInventory inventory;

    [Header("Quests")] // contains active and completed quests (=all)
    public int activeQuestLimit = 10;
    public SyncList<Quest> quests = new SyncList<Quest>();

    // quests //////////////////////////////////////////////////////////////////
    public int GetIndexByName(string questName)
    {
        // (avoid Linq because it is HEAVY(!) on GC and performance)
        for (int i = 0; i < quests.Count; ++i)
            if (quests[i].name == questName)
                return i;
        return -1;
    }

    // helper function to check if the player has completed a quest before
    public bool HasCompleted(string questName)
    {
        // (avoid Linq because it is HEAVY(!) on GC and performance)
        foreach (Quest quest in quests)
            if (quest.name == questName && quest.completed)
                return true;
        return false;
    }

    // count the completed quests
    public int CountIncomplete()
    {
        int count = 0;
        foreach (Quest quest in quests)
            if (!quest.completed)
                ++count;
        return count;
    }

    // helper function to check if a player has an active (not completed) quest
    public bool HasActive(string questName)
    {
        // (avoid Linq because it is HEAVY(!) on GC and performance)
        foreach (Quest quest in quests)
            if (quest.name == questName && !quest.completed)
                return true;
        return false;
    }

    // helper function to check if the player can accept a new quest
    // note: no quest.completed check needed because we have a'not accepted yet'
    //       check
    public bool CanAccept(ScriptableQuest quest)
    {
        // not too many quests yet?
        // has required level?
        // not accepted yet?
        // has finished predecessor quest (if any)?
        return CountIncomplete() < activeQuestLimit &&
               player.level.current >= quest.requiredLevel &&  // has required level?
               GetIndexByName(quest.name) == -1 &&     // not accepted yet?
               (quest.predecessor == null || HasCompleted(quest.predecessor.name));
    }

    [Command]
    public void CmdAccept(int npcQuestIndex)
    {
        // validate
        // use collider point(s) to also work with big entities
        if (player.state == "IDLE" &&
            player.target != null &&
            player.target.health.current > 0 &&
            player.target is Npc npc &&
            0 <= npcQuestIndex && npcQuestIndex < npc.quests.quests.Length &&
            Utils.ClosestDistance(player, npc) <= player.interactionRange)
        {
            ScriptableQuestOffer npcQuest = npc.quests.quests[npcQuestIndex];
            if (npcQuest.acceptHere && CanAccept(npcQuest.quest))
                quests.Add(new Quest(npcQuest.quest));
        }
    }

    // helper function to check if the player can complete a quest
    public bool CanComplete(string questName)
    {
        // has the quest and not completed yet?
        int index = GetIndexByName(questName);
        if (index != -1 && !quests[index].completed)
        {
            // fulfilled?
            Quest quest = quests[index];
            if(quest.IsFulfilled(player))
            {
                // enough space for reward item (if any)?
                return quest.rewardItem == null ||
                       inventory.CanAdd(new ItemOLD(quest.rewardItem), 1);
            }
        }
        return false;
    }

    [Command]
    public void CmdComplete(int npcQuestIndex)
    {
        // validate
        // use collider point(s) to also work with big entities
        if (player.state == "IDLE" &&
            player.target != null &&
            player.target.health.current > 0 &&
            player.target is Npc npc &&
            0 <= npcQuestIndex && npcQuestIndex < npc.quests.quests.Length &&
            Utils.ClosestDistance(player, npc) <= player.interactionRange)
        {
            ScriptableQuestOffer npcQuest = npc.quests.quests[npcQuestIndex];
            if (npcQuest.completeHere)
            {
                int index = GetIndexByName(npcQuest.quest.name);
                if (index != -1)
                {
                    // can complete it? (also checks inventory space for reward, if any)
                    Quest quest = quests[index];
                    if (CanComplete(quest.name))
                    {
                        // call quest.OnCompleted to remove quest items from
                        // inventory, etc.
                        quest.OnCompleted(player);

                        // gain rewards
                        player.gold += quest.rewardGold;
                        player.experience.current += quest.rewardExperience;
                        if (quest.rewardItem != null)
                            inventory.Add(new ItemOLD(quest.rewardItem), 1);

                        // complete quest
                        quest.completed = true;
                        quests[index] = quest;
                    }
                }
            }
        }
    }

    // combat //////////////////////////////////////////////////////////////////
    [Server]
    public void OnKilledEnemy(EntityOLD victim)
    {
        // call OnKilled in all active (not completed) quests
        for (int i = 0; i < quests.Count; ++i)
            if (!quests[i].completed)
                quests[i].OnKilled(player, i, victim);
    }

    // ontrigger ///////////////////////////////////////////////////////////////
    [ServerCallback]
    void OnTriggerEnter(Collider col)
    {
        // quest location? then call OnLocation in active (not completed) quests
        // (we use .CompareTag to avoid .tag allocations)
        if (col.CompareTag("QuestLocation"))
        {
            for (int i = 0; i < quests.Count; ++i)
                if (!quests[i].completed)
                    quests[i].OnLocation(player, i, col);
        }
    }
}
