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
        [SuffixLabel("g", true), ShowInInspector]
        public virtual int weight => data ? data.baseWeight : 0;
        public virtual int volume => data ? data.size.volume : 0;

        public ItemSize size => data ? data.size : default;
    }
}