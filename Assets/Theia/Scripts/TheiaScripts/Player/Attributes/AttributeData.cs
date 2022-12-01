using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Theia.Stats.attributes
{
    // [CreateAssetMenu]
    public class AttributeData : BaseData
    {
        public override bool Contains(BaseData stat) => stat == this || stat.Contains(this);
    }
}
