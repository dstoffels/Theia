using Stats.Anatomy;
using Stats.SkillTypes;
using System;
using System.Collections.Generic;
using InventoryStuff.Armor;
using Sirenix.OdinInspector;
using Stats;
using UnityEngine;
using Stats.IoC;

namespace ArmorTypes
{
    [RequireComponent(typeof(Skills), typeof(Anatomy))]
    public class Armor : StatManager<ArmorSlot, ArmorSlotData>, iSkillConsumer
    {
        [ShowInInspector, ReadOnly]
        public int hindrance => 0;
        [ShowInInspector, ReadOnly]
        public int weight => 0;

        public void Subscribe(iSkillProvider provider)
        {
            if (provider.GetData().name == "Armor")
                provider.AddConsumer(this);
        }

        public void Notify(iSkillProvider provider)
        {
            throw new NotImplementedException();
        }
    }
}
