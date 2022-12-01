using Sirenix.OdinInspector;
using Stats;
using System;
using System.Collections.Generic;

namespace Theia.Items.Base
{
    // TODO: do all items need to be gameobjects?
    public abstract class BaseItem<TData> : DataClientBehaviour<TData>, iItem
        where TData : ItemData
    {
        [ShowInInspector, SuffixLabel("mL", true)]
        public int volume => data.size.volume;
        [ShowInInspector, SuffixLabel("g", true)]
        public virtual int weight => data.baseWeight;
    }
}