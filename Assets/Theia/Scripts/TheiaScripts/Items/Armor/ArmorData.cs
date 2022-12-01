using Stats;
using Theia.Stats.anatomy;
using System.Collections.Generic;
using Theia.Items.Base;

namespace Theia.Items.Armor
{
    public class ArmorData : ItemData
    {
        public ArmorType type;
        // TODO: public List<ArmorSlotData> slots;
        public int baseHindrance;
        public int baseProtection;
        public float coverage = 1;
        public List<BodyPartData> bodyParts;
        // TODO: public List<ArmorMod> modsList;

    }
}
