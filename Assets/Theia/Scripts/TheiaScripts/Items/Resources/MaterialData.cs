using Sirenix.OdinInspector;
using Stats;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Theia.Items.Resources
{
    [CreateAssetMenu(menuName = "Resources/Material")]
    public class MaterialData : BaseData
    {
        [SuffixLabel("g/mL", true)]
        public float density = 1;
        public int hindrance;
        public int damageModifier;
        public int durability = 1000;
        public float conductivity = 1;
        public bool indestructible;
    }
}
