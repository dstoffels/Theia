﻿using InventoryStuff.Armor;
using Mats;
using System.Collections.Generic;
using StatsOLD;

namespace Items.Armor
{
    public class MailleArmor : Item<MetalMat, ArmorItemData>, IArmorItem
    {
        public ArmorSlotData armorSlot => data.armorSlot;

        public List<OrganData> coverage => data.coverage;

        public float hindrance => data.hindrance;

        public float protection => data.protectionFactor;

        public bool isImmuneToSoftSpots => true;
    }
}
