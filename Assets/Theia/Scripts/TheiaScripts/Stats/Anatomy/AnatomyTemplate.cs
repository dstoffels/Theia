﻿using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace StatsOLD
{
    /// <summary>
    /// A simple container with a list of <see cref="OrganData"/> that are populated into an <see cref="Entities"/>'s <see cref="Anatomy"/>.
    /// </summary>
    [CreateAssetMenu(menuName = "Anatomy/Anatomy")]
    public class AnatomyTemplate : SerializedScriptableObject
    {
        [ListDrawerSettings(Expanded = true)]
        public List<OrganData> organList;
    }
}