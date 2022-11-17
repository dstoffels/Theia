using Sirenix.OdinInspector;
using UnityEngine;

namespace Abilities.DamageTypes
{
    public abstract class OffensiveAbility : Ability
    {
        [Title("Offense")]
        public bool requiresEquipment;
        
        [AssetList, Space]
        public DamageType[] damageTypes;
    }

    [CreateAssetMenu(menuName = "Abilities/Ranged Attack")]
    public class RangedAttack : OffensiveAbility
    {
        [Title("Ranged")]
        public bool requiresAmmo;

    }
}