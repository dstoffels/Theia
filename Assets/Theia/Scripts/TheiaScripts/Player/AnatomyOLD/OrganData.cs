using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using Stats;

namespace StatsOLD
{
    /// <summary>
    /// A data container for <see cref="Organ"/>s; Create and add these to <see cref="AnatomyTemplate"/>s to populate an <see cref="Entity"/>'s <see cref="Anatomy"/>
    /// </summary>
    public class OrganData : SerializedScriptableObject
    {
        public bool isVital;
        [SuffixLabel("%", Overlay = true)]
        public float chanceToHit;
        public float maxBloodLossPerMin;

        [ListDrawerSettings(Expanded = true)]
        public List<OrganData> dependents;
    }
}