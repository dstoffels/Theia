using Stats;
using Theia.Stats.anatomy;
using System.Collections.Generic;
using Theia.Items.Base;
using UnityEngine;
using Sirenix.OdinInspector;
using Theia.Stats.armor;

namespace Theia.Items.Armor
{
    [CreateAssetMenu(menuName = "Armor/Armor Item")]
    public class ArmorData : ItemData
    {
        public int baseHindrance;
        public int baseProtection;
        public ArmorType type;
        // TODO: public List<ArmorMod> modsList;

        [DictionaryDrawerSettings(KeyLabel = "Armor Slot", ValueLabel = "Body Parts")]
        public Dictionary<ArmorSlotData, List<BodyPartData>> slots = new Dictionary<ArmorSlotData, List<BodyPartData>>();

    }
}
