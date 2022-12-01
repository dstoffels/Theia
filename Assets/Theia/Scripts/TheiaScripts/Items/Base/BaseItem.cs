using Sirenix.OdinInspector;
using Stats;
using System;
using System.Collections.Generic;

namespace Theia.Items.Base
{
    public abstract class BaseItem<TData> : LinkableObject<TData>, iItem
        where TData : ItemData
    {
        public int volume => data.size.volume;
        public virtual int weight => data.baseWeight;

        public BaseItem(TData data) => this.data = data;
    }
}