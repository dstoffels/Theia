
using Sirenix.OdinInspector;
using UnityEngine;

namespace Abilities.DamageTypes
{
    [CreateAssetMenu(menuName = "Abilities/Melee Attack")]
    public class MeleeAttack : OffensiveAbility
    {
        [Title("Melee")]
        public float damageMultiplier = 1;
    }
}