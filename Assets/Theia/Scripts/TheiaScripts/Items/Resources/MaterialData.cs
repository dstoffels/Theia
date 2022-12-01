using Stats;
using System;
using System.Collections.Generic;

namespace Theia.Items.Resources
{
    public class MaterialData : BaseData
    {
        public float density = 1;
        public int hindrance;
        public int damageModifier;
        public int durability = 1000;
        public float conductivity = 1;
        public bool indestructible;
    }
}
