using Sirenix.OdinInspector;
using System.Collections.Generic;
using Items.Armor;
using Stats.SkillTypes;
using UnityEngine;

namespace InventoryStuff.Armor
{
    [RequireComponent(typeof(Skills))]
    public class Armor : SerializedMonoBehaviour
    {
        public ArmorSlotTemplate template;

        [ReadOnly]
        public ArmorSlots armorSlots = new ArmorSlots();

        [ShowInInspector]
        public ProtectedOrgans protectedOrgans => ProtectedOrgans.Get(armorSlots);

        [ShowInInspector]
        public float hindrance => Mathf.Max(0, GetTotalHindrance() - armorLevel);

        private float armorLevel => skills[template.armorSkill].level;

        [ShowInInspector] public float weight => GetTotalArmorWeight();

        private Skills _skills;
        private Skills skills => _skills ?? (_skills = GetComponent<Skills>());

        private float GetTotalHindrance()
        {
            float total = 0;
            foreach (var slot in armorSlots.Values)
                total += slot.slotHindrance;
            return total;
        }

        private float GetTotalArmorWeight()
        {
            float total = 0;
            foreach (var slot in armorSlots.Values)
                total += slot.slotWeight;
            return total;
        }

        private void OnValidate()
        {
            armorSlots.PopulateSlotsFromTemplate(template);
        }
    }
}