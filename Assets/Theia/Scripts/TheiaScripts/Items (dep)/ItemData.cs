using Sirenix.OdinInspector;
using Stats;

namespace Items
{
    public class ItemData : BaseData
    {
        public int maximumStackSize = 1;

        [InfoBox("An item's base weight is derived from how much it would weigh were it made from its standard material (i.e. A sword's weight is based on steel)", InfoMessageType.None), SuffixLabel("g", Overlay = true)]
        public float baseWeight;

        [InlineProperty]
        public ItemSize size;
    }

    public struct ItemSize
    {
        [SuffixLabel("cm", Overlay = true)]
        public int height, width, depth;

        [ShowInInspector, SuffixLabel("mL", Overlay = true)]
        public float volume => width * height * depth;
    }
}
