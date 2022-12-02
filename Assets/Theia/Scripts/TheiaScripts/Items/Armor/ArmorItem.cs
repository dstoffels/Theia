using Sirenix.OdinInspector;
using System.Collections.Generic;
using Theia.Items.Base;
using Theia.Items.Resources;
using Theia.Stats.armor;
using UnityEngine;

namespace Theia.Items.Armor
{
    // TODO: must prevent duplicate refs of the GameObject
    public class ArmorItem : BreakableItem<ArmorData>
    {
        public ArmorType type => data.type;

        public int splitWeight => Mathf.RoundToInt(weight / data.slots.Count);

        [ShowInInspector]
        public int hindrance => data && material ? (int)((data.baseHindrance + material.hindrance) / quality) : 0;
        [ShowInInspector]
        public int damageReduction => data && material ? (int)(data.baseProtection + material.damageModifier * quality) : 0;
    }
}
