using Theia.Items.Base;
using Theia.Items.Resources;

namespace Theia.Items.Armor
{
    public class ArmorItem : BreakableItem<ArmorData>
    {
        public ArmorType type => data.type;
        public int hindrance => (int)((data.baseHindrance + material.hindrance) / quality);
        public int damageReduction => (int)(data.baseProtection + material.damageModifier * quality);
        
        public ArmorItem(ArmorData data, MaterialData material, float quality) : base(data, material, quality) { }
    }
}
