using Sirenix.OdinInspector;
using Stats;
using System.Collections.Generic;
using UnityEngine;
using Stats.SkillTypes;

namespace InventoryStuff.Armor
{
    [CreateAssetMenu(menuName = "Armor/Armor Slot Template")]
    public class ArmorSlotTemplate : SerializedScriptableObject
    {
        [ReadOnly]
        public SkillData armorSkill;

        [AssetList]
        public List<ArmorSlotData> slots;
    }
}