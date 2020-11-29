using UnityEngine;

[CreateAssetMenu(menuName="uMMORPG Skill/Target Heal", order=999)]
public class TargetHealSkill : HealSkill
{
    public bool canHealSelf = true;
    public bool canHealOthers = false;

    // helper function to determine the target that the skill will be cast on
    // (e.g. cast on self if targeting a monster that isn't healable)
    Entity CorrectedTarget(Entity caster)
    {
        // targeting nothing? then try to cast on self
        if (caster.target == null)
            return canHealSelf ? caster : null;

        // targeting self?
        if (caster.target == caster)
            return canHealSelf ? caster : null;

        // targeting someone of same type? buff them or self
        if (caster.target.GetType() == caster.GetType())
        {
            if (canHealOthers)
                return caster.target;
            else if (canHealSelf)
                return caster;
            else
                return null;
        }

        // no valid target? try to cast on self or don't cast at all
        return canHealSelf ? caster : null;
    }

    public override bool CheckTarget(Entity caster)
    {
        // correct the target
        caster.target = CorrectedTarget(caster);

        // can only buff the target if it's not dead
        return caster.target != null && caster.target.health.current > 0;
    }

    // (has corrected target already)
    public override bool CheckDistance(Entity caster, int skillLevel, out Vector3 destination)
    {
        // check distance to corrected target (without setting the target)
        // this way we can call CheckDistance without setting the corrected
        // target in CheckTarget first. this is needed for skillbar.interactable
        Entity target = CorrectedTarget(caster);

        // target still around?
        if (target != null)
        {
            destination = Utils.ClosestPoint(target, caster.transform.position);
            return Utils.ClosestDistance(caster, target) <= castRange.Get(skillLevel);
        }
        destination = caster.transform.position;
        return false;
    }

    // (has corrected target already)
    public override void Apply(Entity caster, int skillLevel)
    {
        // can't heal dead people
        if (caster.target != null && caster.target.health.current > 0)
        {
            caster.target.health.current += healsHealth.Get(skillLevel);
            caster.target.mana.current += healsMana.Get(skillLevel);

            // show effect on target
            SpawnEffect(caster, caster.target);
        }
    }
}
