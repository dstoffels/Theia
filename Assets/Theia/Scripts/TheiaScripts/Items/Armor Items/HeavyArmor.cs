using InventoryStuff.Armor;
using Mats;
using Sirenix.OdinInspector;
using StatsOLD;
using System.Collections.Generic;
using UnityEngine;

namespace Items.Armor
{
    public class HeavyArmor : Item<RigidMat, ArmorItemData>, IArmorItem
    {
        public ArmorAccessory accessory;
        /*private bool hasAccessory => accessory != null;*/

        public ArmorSlotData armorSlot => data.armorSlot;

        public List<OrganData> coverage => GetAllCoverage();

        [ShowInInspector]
        public float hindrance => Mathf.FloorToInt(data.hindrance * material.hindranceMultiplier);

        public float protection => data.protectionFactor;

        public bool isImmuneToSoftSpots => accessory ? accessory.isImmuneToSoftSpots : false;

        private List<OrganData> GetAllCoverage()
        {
            var list = data.coverage;
            if (accessory && accessory.extraCoverage)
                list.Add(accessory.extraCoverage);
            return list;
        }
    }

    public class ArmorAccessory : SerializedScriptableObject
    {
        public OrganData extraCoverage;
        public bool isImmuneToSoftSpots;
        public float hindrance;
    }
}
