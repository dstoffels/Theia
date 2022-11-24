using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Stats.ArchetypeTypes
{
    [CreateAssetMenu(menuName = "Archetypes/Crosstype")]
    public class CrossType : Archetype
    {
        public Domain[] domains = new Domain[2];
    }
}
