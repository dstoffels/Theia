using Sirenix.OdinInspector;
using System.Collections.Generic;
using Theia.Items.Resources;
using UnityEngine;

namespace Theia.Items.Base
{
    /// <summary>
    /// Material Items combine the stats of the item's data and the material modifiers.
    /// </summary>
    public abstract class MaterialItem<TData> : BaseItem<TData>
        where TData : ItemData
    {
        public MaterialData material;
        public override string name => $"{material.name} {base.name}";
        public override int weight => (int)(data.baseWeight * material.density);

        protected MaterialItem(TData data, MaterialData material) : base(data)
        {
            this.material = material;
        }
    }
}
