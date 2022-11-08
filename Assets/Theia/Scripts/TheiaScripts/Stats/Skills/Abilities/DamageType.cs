using Sirenix.OdinInspector;
using StatsOLD;
using System.Collections.Generic;
using UnityEngine;

namespace Abilities.DamageTypes
{
    [CreateAssetMenu(menuName = "Abilities/Damage Type")]
    public abstract class DamageType : SerializedScriptableObject
    {
        public float armorPiercingFactor;
        public float damageFactor = 1;
        public float heatFactor;

        public abstract void DamageEffect(EntityOLD target);
    }

    public class LacerationDamage : DamageType
    {
        public override void DamageEffect(EntityOLD target)
        {
            
        }
    }

    public class PunctureDamage : DamageType
    {
        public override void DamageEffect(EntityOLD target)
        {
            throw new System.NotImplementedException();
        }
    }

    public class BurnDamage : DamageType
    {
        public override void DamageEffect(EntityOLD target)
        {
            throw new System.NotImplementedException();
        }
    }

    public class FrostDamage : DamageType
    {
        public override void DamageEffect(EntityOLD target)
        {
            throw new System.NotImplementedException();
        }
    }

    public class StaminaDamage : DamageType
    {
        public override void DamageEffect(EntityOLD target)
        {
            throw new System.NotImplementedException();
        }
    }

    public class ManaDamage : DamageType
    {
        public override void DamageEffect(EntityOLD target)
        {
            throw new System.NotImplementedException();
        }
    }

    public class BloodDamage : DamageType
    {
        public override void DamageEffect(EntityOLD target)
        {
            throw new System.NotImplementedException();
        }
    }
}