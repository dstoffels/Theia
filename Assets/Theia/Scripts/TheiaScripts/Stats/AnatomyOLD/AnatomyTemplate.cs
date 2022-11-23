using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using Stats;

namespace StatsOLD
{
    /// <summary>
    /// A simple container with a list of <see cref="OrganData"/> that are populated into an <see cref="Entity"/>'s <see cref="Anatomy"/>.
    /// </summary>
    public class AnatomyTemplate : ScriptableObject
    {
        [ListDrawerSettings(Expanded = true)]
        public List<OrganData> organList;
    }
}