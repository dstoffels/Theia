using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections.Generic;
using Theia.IoC;
using Theia.Items.Armor;
using Theia.Stats.anatomy;
using Theia.Stats.gear;

namespace Theia.Stats.armor
{
    [HideReferenceObjectPicker]
    public class ArmorSlot : DataClient<ArmorSlotData>, iArmorProvider, iWeightProvider, iWearableItemSlot<ArmorItem>
    {
        [ShowInInspector, ReadOnly]
        private Dictionary<ArmorType, ArmorItem> slots;

        public ArmorSlot()
        {
            slots = new Dictionary<ArmorType, ArmorItem>();
            foreach (var type in ArmorType.all)
                slots.Add(type, null);
        }

        public ArmorItem Equip(ArmorItem armorItem)
        {
            ArmorItem currentArmor= null;
            if (slots[armorItem.type] != null)
                currentArmor = slots[armorItem.type];
            slots[armorItem.type] = armorItem;
            consumers.Notify(this);
            return currentArmor;
        }

        public ArmorItem Remove(ArmorItem armorItem)
        {
            if (slots[armorItem.type] == armorItem)
            {
                slots[armorItem.type] = null;
                consumers.Notify(this);
                return armorItem;
            }
            return null;
        }


        // CONSUMER INTERFACE
        private ArmorSlotConsumers consumers =  new ArmorSlotConsumers();
        public void AddConsumer(iArmorConsumer consumer) => consumers.Add(consumer); 

        // TODO: update to implement armor "soft spots"? pass in action along with bodypart?
        public int GetDamageReduction(BodyPartData bodypart) => 
            utils.Sum<ArmorItem>(slots.Values, item => item && item.data.slots[data].Contains(bodypart) ? item.damageReduction : 0);

        public int GetWeight() => utils.Sum<ArmorItem>(slots.Values, item => item ? item.splitWeight : 0);
        public int GetHindrance() => utils.Sum<ArmorItem>(slots.Values, item => item ? item.hindrance: 0);
        public BaseData GetData() => data;
    }
}
