using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Theia.Items.refactor
{
    [CreateAssetMenu]
    public class InventoryTemplate : SerializedScriptableObject
    {
        [AssetList]
        public List<GearSlotData> slots;
    }
}
