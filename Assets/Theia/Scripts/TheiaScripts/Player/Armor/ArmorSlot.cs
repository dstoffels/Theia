using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections.Generic;
using Theia.IoC;
using Theia.Items.Armor;
using Theia.Stats.anatomy;

namespace Theia.Stats.armor
{
    [HideReferenceObjectPicker]
    public class ArmorSlot : DataClient<ArmorSlotData>, iArmorProvider
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
            consumers.Notify(this);
            return currentArmor;
        }


        // CONSUMER INTERFACE
        private ArmorSlotConsumers consumers =  new ArmorSlotConsumers();
        public void AddConsumer(iArmorConsumer consumer) => consumers.Add(consumer); 

        // TODO: update to implement armor "soft spots"?
        public int GetDamageReduction(BodyPartData bodypart)
        {
            int total = 0;
            foreach (var slot in slots.Values)
                if(slot != null && slot.data.bodyParts.Contains(bodypart))
                    total += slot.damageReduction;
            return total;
        }

        public BaseData GetData() => data;

    }
}
