using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;

[Serializable]
public struct SkillbarEntry
{
    public string reference;
    public KeyCode hotKey;
}

[RequireComponent(typeof(PlayerEquipment))]
[RequireComponent(typeof(PlayerInventory))]
[RequireComponent(typeof(PlayerSkills))]
public class PlayerSkillbar : NetworkBehaviour
{
    [Header("Components")]
    public PlayerEquipment equipment;
    public PlayerInventory inventory;
    public PlayerSkills skills;

    [Header("Skillbar")]
    public SkillbarEntry[] slots =
    {
        new SkillbarEntry{reference="", hotKey=KeyCode.Alpha1},
        new SkillbarEntry{reference="", hotKey=KeyCode.Alpha2},
        new SkillbarEntry{reference="", hotKey=KeyCode.Alpha3},
        new SkillbarEntry{reference="", hotKey=KeyCode.Alpha4},
        new SkillbarEntry{reference="", hotKey=KeyCode.Alpha5},
        new SkillbarEntry{reference="", hotKey=KeyCode.Alpha6},
        new SkillbarEntry{reference="", hotKey=KeyCode.Alpha7},
        new SkillbarEntry{reference="", hotKey=KeyCode.Alpha8},
        new SkillbarEntry{reference="", hotKey=KeyCode.Alpha9},
        new SkillbarEntry{reference="", hotKey=KeyCode.Alpha0},
    };

    public override void OnStartLocalPlayer()
    {
        // load skillbar after player data was loaded
        Load();
    }

    public override void OnStopClient()
    {
        if (isLocalPlayer)
            Save();
    }

    // skillbar ////////////////////////////////////////////////////////////////
    //[Client] <- disabled while UNET OnDestroy isLocalPlayer bug exists
    void Save()
    {
        // save skillbar to player prefs (based on player name, so that
        // each character can have a different skillbar)
        for (int i = 0; i < slots.Length; ++i)
            PlayerPrefs.SetString(name + "_skillbar_" + i, slots[i].reference);

        // force saving playerprefs, otherwise they aren't saved for some reason
        PlayerPrefs.Save();
    }

    [Client]
    void Load()
    {
        Debug.Log("loading skillbar for " + name);
        List<SkillOLD> learned = skills.skills.Where(skill => skill.level > 0).ToList();
        for (int i = 0; i < slots.Length; ++i)
        {
            // try loading an existing entry
            if (PlayerPrefs.HasKey(name + "_skillbar_" + i))
            {
                string entry = PlayerPrefs.GetString(name + "_skillbar_" + i, "");

                // is this a valid item/equipment/learned skill?
                // (might be an old character's playerprefs)
                // => only allow learned skills (in case it's an old character's
                //    skill that we also have, but haven't learned yet)
                if (skills.HasLearned(entry) ||
                    inventory.GetItemIndexByName(entry) != -1 ||
                    equipment.GetItemIndexByName(entry) != -1)
                {
                    slots[i].reference = entry;
                }
            }
            // otherwise fill with default skills for a better first impression
            else if (i < learned.Count)
            {
                slots[i].reference = learned[i].name;
            }
        }
    }

    // drag & drop /////////////////////////////////////////////////////////////
    void OnDragAndDrop_InventorySlot_SkillbarSlot(int[] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        slots[slotIndices[1]].reference = inventory.slots[slotIndices[0]].item.name; // just save it clientsided
    }

    void OnDragAndDrop_EquipmentSlot_SkillbarSlot(int[] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        slots[slotIndices[1]].reference = equipment.slots[slotIndices[0]].item.name; // just save it clientsided
    }

    void OnDragAndDrop_SkillsSlot_SkillbarSlot(int[] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        slots[slotIndices[1]].reference = skills.skills[slotIndices[0]].name; // just save it clientsided
    }

    void OnDragAndDrop_SkillbarSlot_SkillbarSlot(int[] slotIndices)
    {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        // just swap them clientsided
        string temp = slots[slotIndices[0]].reference;
        slots[slotIndices[0]].reference = slots[slotIndices[1]].reference;
        slots[slotIndices[1]].reference = temp;
    }

    void OnDragAndClear_SkillbarSlot(int slotIndex)
    {
        slots[slotIndex].reference = "";
    }
}
