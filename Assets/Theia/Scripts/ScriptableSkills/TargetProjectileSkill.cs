// TargetProjectileSkill
// -> assumes that all Entities have an Equipment component
using UnityEngine;
using Mirror;

[CreateAssetMenu(menuName="uMMORPG Skill/Target Projectile", order=999)]
public class TargetProjectileSkill : DamageSkill
{
    [Header("Projectile")]
    public ProjectileSkillEffect projectile; // Arrows, Bullets, Fireballs, ...

    bool HasRequiredWeaponAndAmmo(Entity caster)
    {
        // requires no weapon category?
        // then we can't find weapon and check ammo. just allow it.
        // (monsters have no weapon requirements and don't even have an
        //  equipment component)
        if (string.IsNullOrWhiteSpace(requiredWeaponCategory))
            return true;

        int weaponIndex = caster.equipment.GetEquippedWeaponIndex();
        if (weaponIndex != -1)
        {
            // no ammo required, or has that ammo equipped?
            WeaponItem itemData = (WeaponItem)caster.equipment.slots[weaponIndex].item.data;
            return itemData.requiredAmmo == null ||
                   caster.equipment.GetItemIndexByName(itemData.requiredAmmo.name) != -1;
        }
        return false;
    }

    void ConsumeRequiredWeaponsAmmo(Entity caster)
    {
        // requires no weapon category?
        // then we can't find weapon and check ammo. just allow it.
        // (monsters have no weapon requirements and don't even have an
        //  equipment component)
        if (string.IsNullOrWhiteSpace(requiredWeaponCategory))
            return;

        int weaponIndex = caster.equipment.GetEquippedWeaponIndex();
        if (weaponIndex != -1)
        {
            // no ammo required, or has that ammo equipped?
            WeaponItem itemData = (WeaponItem)caster.equipment.slots[weaponIndex].item.data;
            if (itemData.requiredAmmo != null)
            {
                int ammoIndex = caster.equipment.GetItemIndexByName(itemData.requiredAmmo.name);
                if (ammoIndex != 0)
                {
                    // reduce it
                    ItemSlot slot = caster.equipment.slots[ammoIndex];
                    --slot.amount;
                    caster.equipment.slots[ammoIndex] = slot;
                }
            }
        }
    }

    public override bool CheckSelf(Entity caster, int skillLevel)
    {
        // check base and ammo
        return base.CheckSelf(caster, skillLevel) &&
               HasRequiredWeaponAndAmmo(caster);
    }

    public override bool CheckTarget(Entity caster)
    {
        // target exists, alive, not self, oktype?
        return caster.target != null && caster.CanAttack(caster.target);
    }

    public override bool CheckDistance(Entity caster, int skillLevel, out Vector3 destination)
    {
        // target still around?
        if (caster.target != null)
        {
            destination = Utils.ClosestPoint(caster.target, caster.transform.position);
            return Utils.ClosestDistance(caster, caster.target) <= castRange.Get(skillLevel);
        }
        destination = caster.transform.position;
        return false;
    }

    public override void Apply(Entity caster, int skillLevel)
    {
        // consume ammo if needed
        ConsumeRequiredWeaponsAmmo(caster);

        // spawn the skill effect. this can be used for anything ranging from
        // blood splatter to arrows to chain lightning.
        // -> we need to call an RPC anyway, it doesn't make much of a diff-
        //    erence if we use NetworkServer.Spawn for everything.
        // -> we try to spawn it at the weapon's projectile mount
        if (projectile != null)
        {
            GameObject go = Instantiate(projectile.gameObject, caster.skills.effectMount.position, caster.skills.effectMount.rotation);
            ProjectileSkillEffect effect = go.GetComponent<ProjectileSkillEffect>();
            effect.target = caster.target;
            effect.caster = caster;
            effect.damage = damage.Get(skillLevel);
            effect.stunChance = stunChance.Get(skillLevel);
            effect.stunTime = stunTime.Get(skillLevel);
            NetworkServer.Spawn(go);
        }
        else Debug.LogWarning(name + ": missing projectile");
    }
}
