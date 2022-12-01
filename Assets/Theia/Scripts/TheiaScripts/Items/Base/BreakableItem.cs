using Theia.Items.Resources;
using UnityEngine;

namespace Theia.Items.Base
{
    public abstract class BreakableItem<TData> : MaterialItem<TData>, iDamageable
        where TData: ItemData
    {
        public float quality { get; set; }
        public int condition { get; private set; }
        public int durability => (int)(material.durability * quality);
        public bool broken => condition == 0;
        public void Damage(int amt) => condition = Mathf.Max(0, condition - amt);

        protected BreakableItem(TData data, MaterialData material, float quality) : base(data, material)
        {
            this.quality = quality;
            condition = durability;
        }
    }
}
