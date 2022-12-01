using Sirenix.OdinInspector;
using Stats;
using System;
using System.Collections.Generic;

namespace Theia.Items.Base
{
    // TODO: do all items need to be gameobjects?
    public abstract class BaseItem<TData> : DataClient<TData>, iItem
        where TData : ItemData
    {
        public int volume => data.size.volume;
        public virtual int weight => data.baseWeight;

        public BaseItem(TData data) => this.data = data;
    }
}