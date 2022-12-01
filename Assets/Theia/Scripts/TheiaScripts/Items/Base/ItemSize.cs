using Sirenix.OdinInspector;

namespace Theia.Items.Base
{
    public struct ItemSize
    {
        [SuffixLabel("cm", Overlay = true)]
        public int height, width, depth;

        [ShowInInspector, SuffixLabel("mL", Overlay = true)]
        public int volume => width * height * depth;
    }
}
