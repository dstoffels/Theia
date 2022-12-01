using Sirenix.OdinInspector;
using UnityEngine;

namespace Theia.Stats.archetypes
{
    [CreateAssetMenu(menuName = "Archetypes/Crosstype")]
    public class CrossType : Archetype
    {
        [ListDrawerSettings(IsReadOnly =true)]
        public Domain[] domains = new Domain[2];
    }
}
