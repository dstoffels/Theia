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
        public override int weight => data && material ? (int)(data.baseWeight * material.density) : 0;

        [Button]
        public void Init(TData data, MaterialData material)
        {
            base.Init(data);
            this.material = material;
            name = $"{material.name} {data.name}";
        }
    }
}
