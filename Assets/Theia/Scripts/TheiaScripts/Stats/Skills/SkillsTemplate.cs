// A simple scriptable object for making skill templates, mainly for entities (NPC's, creatures/monsters).
// Player characters begin with a limited number of skills, the rest are unlocked as the player progresses.

using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace StatsOLD
{
    [CreateAssetMenu(menuName = "Skills/Skill Template")]
    public class SkillsTemplate : SerializedScriptableObject
    {
        public List<SkillData> skillsList = new List<SkillData>();
    }
}
