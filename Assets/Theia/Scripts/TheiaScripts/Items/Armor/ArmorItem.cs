using Sirenix.OdinInspector;
using Theia.Items.Base;
using Theia.Items.Resources;

namespace Theia.Items.Armor
{
    // TODO: must prevent duplicate refs of the GameObject
    public class ArmorItem : BreakableItem<ArmorData>
    {
        public ArmorType type => data.type;
        [ShowInInspector]
        public int hindrance => (int)((data.baseHindrance + material.hindrance) / quality);
        [ShowInInspector]
        public int damageReduction => (int)(data.baseProtection + material.damageModifier * quality);
    }
}
