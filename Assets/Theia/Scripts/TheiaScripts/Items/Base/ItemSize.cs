using Sirenix.OdinInspector;
using UnityEngine;

namespace Theia.Items.Base
{
    [InlineProperty]
    public struct ItemSize
    {
        [SuffixLabel("cm", Overlay = true)]
        public int height, width, depth;

        [ShowInInspector, SuffixLabel("mL", Overlay = true)]
        public int volume => width * height * depth;

        public int greatestDimension => Mathf.Max(height, Mathf.Max(width, depth));
    }
}
