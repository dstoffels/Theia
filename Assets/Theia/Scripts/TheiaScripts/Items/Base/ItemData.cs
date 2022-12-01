using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Theia.Items.Base
{
    public class ItemData : BaseData
    {
        public int maxStackSize = 1;
        [SuffixLabel("g", true)]
        public int baseWeight;
        [InlineProperty]
        public ItemSize size;
        public Sprite image;
    }
    public class MagicItemData : ItemData { }
}
