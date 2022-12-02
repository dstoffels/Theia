using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections.Generic;
using Theia.IoC;
using Theia.Items.Armor;
using Theia.Stats.anatomy;

namespace Theia.Stats.armor
{
    [HideReferenceObjectPicker]
    public class ArmorSlot : DataClient<ArmorSlotData>, iArmorProvider, iWeightProvider
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
                return armorItem;
            }
            return null;
        }


        // CONSUMER INTERFACE
        private ArmorSlotConsumers consumers =  new ArmorSlotConsumers();
        public void AddConsumer(iArmorConsumer consumer) => consumers.Add(consumer); 

        // TODO: update to implement armor "soft spots"? pass in action along with bodypart?
        public int GetDamageReduction(BodyPartData bodypart)
        {
            int total = 0;
            foreach (var armor in slots.Values)
                if(armor != null && armor.data.slots[data].Contains(bodypart))
                    total += armor.damageReduction;
            return total;
        }

        public BaseData GetData() => data;

        public int GetWeight()
        {
            int weight = 0;
            foreach (var slot in slots.Values)
                weight += slot ? slot.splitWeight : 0;
            return weight;
        }

        public int GetHindrance()
        {
            int hindrance = 0;
            foreach(var slot in slots.Values)
                hindrance += slot ? slot.hindrance : 0;
            return hindrance;
        }
    }
}
