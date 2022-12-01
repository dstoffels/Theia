using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Stats.ArchetypeTypes
{
    [CreateAssetMenu(menuName = "Archetypes/Specialization")]
    public class SpecType : Archetype
    {
        public Domain domain;
    }
    
}
