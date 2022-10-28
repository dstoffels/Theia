using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace InventoryStuff
{
    [CreateAssetMenu]
    public class InventoryTemplate : SerializedScriptableObject
    {
        [AssetList]
        public List<GearSlotData> slots;
    }
}
