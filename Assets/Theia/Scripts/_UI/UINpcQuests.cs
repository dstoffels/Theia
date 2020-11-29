using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class UINpcQuests : MonoBehaviour
{
    public static UINpcQuests singleton;
    public GameObject panel;
    public UINpcQuestSlot slotPrefab;
    public Transform content;

    public UINpcQuests()
    {
        // assign singleton only once (to work with DontDestroyOnLoad when
        // using Zones / switching scenes)
        if (singleton == null) singleton = this;
    }

    void Update()
    {
        Player player = Player.localPlayer;

        // use collider point(s) to also work with big entities
        if (player != null &&
            player.target != null &&
            player.target is Npc npc &&
            Utils.ClosestDistance(player, player.target) <= player.interactionRange)
        {
            // instantiate/destroy enough slots
            List<ScriptableQuest> questsAvailable = npc.quests.QuestsVisibleFor(player);
            UIUtils.BalancePrefabs(slotPrefab.gameObject, questsAvailable.Count, content);

            // refresh all
            for (int i = 0; i < questsAvailable.Count; ++i)
            {
                ScriptableQuest npcQuest = questsAvailable[i];
                UINpcQuestSlot slot = content.GetChild(i).GetComponent<UINpcQuestSlot>();

                // find quest index in original npc quest list (unfiltered)
                int npcIndex = npc.quests.GetIndexByName(questsAvailable[i].name);

                // find quest index in player quest list
                int questIndex = player.quests.GetIndexByName(npcQuest.name);
                if (questIndex != -1)
                {
                    // running quest: shows description with current progress
                    // instead of static one
                    Quest quest = player.quests.quests[questIndex];
                    ScriptableItem reward = npcQuest.rewardItem;
                    bool hasSpace = reward == null || player.inventory.CanAdd(new Item(reward), 1);

                    // description + not enough space warning (if needed)
                    slot.descriptionText.text = quest.ToolTip(player);
                    if (!hasSpace)
                        slot.descriptionText.text += "\n<color=red>Not enough inventory space!</color>";

                    slot.actionButton.interactable = player.quests.CanComplete(quest.name);
                    slot.actionButton.GetComponentInChildren<Text>().text = "Complete";
                    slot.actionButton.onClick.SetListener(() => {
                        player.quests.CmdComplete(npcIndex);
                        panel.SetActive(false);
                    });
                }
                else
                {
                    // new quest
                    slot.descriptionText.text = new Quest(npcQuest).ToolTip(player);
                    slot.actionButton.interactable = true;
                    slot.actionButton.GetComponentInChildren<Text>().text = "Accept";
                    slot.actionButton.onClick.SetListener(() => {
                        player.quests.CmdAccept(npcIndex);
                    });
                }
            }
        }
        else panel.SetActive(false);
    }
}
