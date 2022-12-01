using Items;
using Stats;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Theia.Items.Base
{
    public class ItemData : BaseData
    {
        public int maxStackSize = 1;
        public int baseWeight;
        public ItemSize size;
        public Sprite image;
    }
    public class MagicItemData : ItemData { }
}
