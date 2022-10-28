using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Stats
{
    public class StatData : SerializedScriptableObject
    {
        [TextArea(2, 10), PropertyOrder(999)]
        public string tooltip;
    }
}
