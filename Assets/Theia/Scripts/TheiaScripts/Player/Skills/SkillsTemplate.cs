// A simple scriptable object for making skill templates, mainly for entities (NPC's, creatures/monsters).
// Player characters begin with a limited number of skills, the rest are unlocked as the player progresses.

using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Stats.SkillTypes;

namespace Stats
{
    [CreateAssetMenu(menuName = "Skills/Skill Template")]
    public class SkillsTemplate : StatTemplate<SkillData>
    {
    }
}
