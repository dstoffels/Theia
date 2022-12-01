using Sirenix.OdinInspector;
using Stats;
using System.Collections.Generic;
using Theia.Items.Armor;

namespace Theia.Stats.ArmorScripts
{
    [HideReferenceObjectPicker]
    public class ArmorSlot : DataClient<ArmorSlotData>
    {
        [ShowInInspector, ReadOnly]
        private Dictionary<ArmorType, ArmorItem> slots;

        public ArmorSlot()
        {
            slots = new Dictionary<ArmorType, ArmorItem>();
            foreach (var type in ArmorType.all)
                slots.Add(type, null);
        }

        [Button]
        public ArmorItem Equip(ArmorItem newArmor)
        {
            ArmorItem currentArmor= null;
            if (slots[newArmor.type] != null)
                currentArmor = slots[newArmor.type];
            slots[newArmor.type] = newArmor;
            return currentArmor;
        }
    }
}
