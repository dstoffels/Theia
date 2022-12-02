using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Theia.Stats.gear
{
    [CreateAssetMenu]
    public class InventoryTemplate : SerializedScriptableObject
    {
        [AssetList]
        public List<GearSlotData> slots;
    }
}
