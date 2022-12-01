using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Theia.Stats.ArmorScripts
{
    public class Armor : MonoBehaviour
    {
        private Dictionary<ArmorSlotData, ArmorSlot> cache;

        [ShowInInspector]
        public Dictionary<ArmorSlotData, ArmorSlot> slots
        {
            get
            {
                if(cache is null)
                {
                    cache = new Dictionary<ArmorSlotData, ArmorSlot>();
                    ArmorSlotData[] all = Resources.LoadAll<ArmorSlotData>("");
                    foreach (var data in all)
                    {
                        var slot = new ArmorSlot();
                        slot.Init(data);
                        cache.Add(data, slot);
                    }
                }
                return cache;
            }
        }

    }
}
