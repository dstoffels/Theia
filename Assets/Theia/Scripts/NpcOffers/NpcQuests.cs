using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// talk-to-npc quests work by adding the same quest to two npcs, one with
// accept=true and complete=false, the other with accept=false and complete=true
[Serializable]
public class ScriptableQuestOffer
{
    public ScriptableQuest quest;
    public bool acceptHere = true;
    public bool completeHere = true;
}

public class NpcQuests : NpcOffer
{
    [Header("Text Meshes")]
    public TextMeshPro questOverlay;

    [Header("Quests")]
    public ScriptableQuestOffer[] quests;

    public override bool HasOffer(Player player) =>
        QuestsVisibleFor(player).Count > 0;

    public override string GetOfferName() => "Quests";

    public override void OnSelect(Player player)
    {
        UINpcQuests.singleton.panel.SetActive(true);
        UINpcDialogue.singleton.panel.SetActive(false);
    }

    // helper function to find a quest index by name
    public int GetIndexByName(string questName)
    {
        // (avoid Linq because it is HEAVY(!) on GC and performance)
        for (int i = 0; i < quests.Length; ++i)
            if (quests[i].quest.name == questName)
                return i;
        return -1;
    }

    // helper function to filter the quests that are shown for a player
    // -> all quests that:
    //    - can be started by the player
    //    - or were already started but aren't completed yet
    public List<ScriptableQuest> QuestsVisibleFor(Player player)
    {
        // search manually. Linq is HEAVY(!) on GC and performance
        List<ScriptableQuest> visibleQuests = new List<ScriptableQuest>();
        foreach (ScriptableQuestOffer entry in quests)
            if (entry.acceptHere && player.quests.CanAccept(entry.quest) ||
                entry.completeHere && player.quests.HasActive(entry.quest.name))
                visibleQuests.Add(entry.quest);
        return visibleQuests;
    }

    public bool CanPlayerCompleteAnyQuestHere(PlayerQuests playerQuests)
    {
        // check manually. Linq.Any() is HEAVY(!) on GC and performance
        foreach (ScriptableQuestOffer entry in quests)
            if (entry.completeHere && playerQuests.CanComplete(entry.quest.name))
                return true;
        return false;
    }

    public bool CanPlayerAcceptAnyQuestHere(PlayerQuests playerQuests)
    {
        // check manually. Linq.Any() is HEAVY(!) on GC and performance
        foreach (ScriptableQuestOffer entry in quests)
            if (entry.acceptHere && playerQuests.CanAccept(entry.quest))
                return true;
        return false;
    }

    void Update()
    {
        // update overlays in any case, except on server-only mode
        // (also update for character selection previews etc. then)
        if (isServerOnly) return;

        if (questOverlay != null)
        {
            // find local player (null while in character selection)
            if (Player.localPlayer != null)
            {
                if (CanPlayerCompleteAnyQuestHere(Player.localPlayer.quests))
                    questOverlay.text = "!";
                else if (CanPlayerAcceptAnyQuestHere(Player.localPlayer.quests))
                    questOverlay.text = "?";
                else
                    questOverlay.text = "";
            }
        }
    }
}
