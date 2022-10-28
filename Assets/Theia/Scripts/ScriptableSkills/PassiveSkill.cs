using UnityEngine;

[CreateAssetMenu(menuName="uMMORPG Skill/Passive Skill", order=999)]
public class PassiveSkill : BonusSkill
{
    public override bool CheckTarget(EntityOLD caster) { return false; }
    public override bool CheckDistance(EntityOLD caster, int skillLevel, out Vector3 destination)
    {
        destination = caster.transform.position;
        return false;
    }
    public override void Apply(EntityOLD caster, int skillLevel) {}
}
