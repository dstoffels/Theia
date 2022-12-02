using Sirenix.OdinInspector;
using Stats;
using System.Collections.Generic;
using Theia.IoC;
using Theia.Items.Armor;
using UnityEngine;

namespace Theia.Stats.armor
{
    public class Armor : DataClientManager<ArmorSlotData, ArmorSlot>, iProviderManager<iArmorProvider>
    {
        [ShowInInspector]
        public int totalWeight { get; private set; }
        [ShowInInspector]
        public int totalHindrance { get; private set; }
        public iArmorProvider[] GetProviders() => all;

        [Button]
        public void Equip(ArmorItem armorItem)
        {
            foreach (var slot in armorItem.data.slots.Keys)
                this[slot].Equip(armorItem);
            SetTotalWeight();
            SetTotalHindrance();
        }

        [Button]
        public ArmorItem Remove(ArmorItem armorItem)
        {
            ArmorItem removedItem = null;
            foreach (var slot in all)
            {
                var temp = slot.Remove(armorItem);
                removedItem = temp ? temp : removedItem;
            }
            SetTotalWeight();
            SetTotalHindrance();
            return removedItem;
        }

        private void SetTotalWeight()
        {
            totalWeight = 0;
            foreach (var armorSlot in all)
                totalWeight += armorSlot.GetWeight();
        }

        private void SetTotalHindrance()
        {
            totalHindrance = 0;
            foreach (var armorSlot in all)
                totalHindrance += armorSlot.GetHindrance();
        }

        private void OnValidate()
        {
            SetTotalWeight();
            SetTotalHindrance();
        }
    }
}
