using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace StatsOLD
{
    /// <summary>
    /// A data container for <see cref="Organ"/>s; Create and add these to <see cref="AnatomyTemplate"/>s to populate an <see cref="Entities"/>'s <see cref="Anatomy"/>
    /// </summary>
    [CreateAssetMenu(menuName = "Anatomy/Organ")]
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

public class Attack
{
    public StatsOLD.OrganData FindOrgan(Player target)
    {
        var roll = DOneHundred.Roll();
        var count = 0f;

        foreach (var organ in target.anatomy.organs.Values)
        {
            count += organ.data.chanceToHit;
            if (roll < count) return organ.data;
        }

        return null;
    }
}