using UnityEngine;
using Mirror;

[RequireComponent(typeof(Level))]
[RequireComponent(typeof(Movement))]
[RequireComponent(typeof(PlayerParty))]
[DisallowMultipleComponent]
public class PlayerSkills : SkillsOLD
{
    [Header("Components")]
    public Level level;
    public Movement movement;
    public PlayerParty party;

    [Header("Skill Experience")]
    [SyncVar] public long skillExperience = 0;

    void Start()
    {
        // do nothing if not spawned (=for character selection previews)
        if (!isServer && !isClient) return;

        // spawn effects for any buffs that might still be active after loading
        // (OnStartServer is too early)
        // note: no need to do that in Entity.Start because we don't load them
        //       with previously casted skills
        if (isServer)
            for (int i = 0; i < buffs.Count; ++i)
                if (buffs[i].BuffTimeRemaining() > 0)
                    buffs[i].data.SpawnEffect(entity, entity);
    }

    [Command]
    public void CmdUse(int skillIndex)
    {
        // validate
        if ((entity.state == "IDLE" || entity.state == "MOVING" || entity.state == "CASTING") &&
            0 <= skillIndex && skillIndex < skills.Count)
        {
            // skill learned and can be casted?
            if (skills[skillIndex].level > 0 && skills[skillIndex].IsReady())
            {
                currentSkill = skillIndex;
            }
        }
    }

    // helper function: try to use a skill and walk into range if necessary
    [Client]
    public void TryUse(int skillIndex, bool ignoreState=false)
    {
        // only if not casting already
        // (might need to ignore that when coming from pending skill where
        //  CASTING is still true)
        if (entity.state != "CASTING" || ignoreState)
        {
            SkillOLD skill = skills[skillIndex];
            if (CastCheckSelf(skill) && CastCheckTarget(skill))
            {
                // check distance between self and target
                Vector3 destination;
                if (CastCheckDistance(skill, out destination))
                {
                    // cast
                    CmdUse(skillIndex);
                }
                else
                {
                    // move to the target first
                    // (use collider point(s) to also work with big entities)
                    float stoppingDistance = skill.castRange * ((PlayerOLD)entity).attackToMoveRangeRatio;
                    movement.Navigate(destination, stoppingDistance);

                    // use skill when there
                    ((PlayerOLD)entity).useSkillWhenCloser = skillIndex;
                }
            }
        }
        else
        {
            ((PlayerOLD)entity).pendingSkill = skillIndex;
        }
    }

    public bool HasLearned(string skillName)
    {
        // has this skill with at least level 1 (=learned)?
        return HasLearnedWithLevel(skillName, 1);
    }

    public bool HasLearnedWithLevel(string skillName, int skillLevel)
    {
        // (avoid Linq because it is HEAVY(!) on GC and performance)
        foreach (SkillOLD skill in skills)
            if (skill.level >= skillLevel && skill.name == skillName)
                return true;
        return false;
    }

    // helper function for command and UI
    // -> this is for learning and upgrading!
    public bool CanUpgrade(SkillOLD skill)
    {
        return skill.level < skill.maxLevel &&
               level.current >= skill.upgradeRequiredLevel &&
               skillExperience >= skill.upgradeRequiredSkillExperience &&
               (skill.predecessor == null || (HasLearnedWithLevel(skill.predecessor.name, skill.predecessorLevel)));
    }

    // -> this is for learning and upgrading!
    [Command]
    public void CmdUpgrade(int skillIndex)
    {
        // validate
        if ((entity.state == "IDLE" || entity.state == "MOVING" || entity.state == "CASTING") &&
            0 <= skillIndex && skillIndex < skills.Count)
        {
            // can be upgraded?
            SkillOLD skill = skills[skillIndex];
            if (CanUpgrade(skill))
            {
                // decrease skill experience
                skillExperience -= skill.upgradeRequiredSkillExperience;

                // upgrade
                ++skill.level;
                skills[skillIndex] = skill;
            }
        }
    }

    // events //////////////////////////////////////////////////////////////////
    [Server]
    public void OnKilledEnemy(EntityOLD victim)
    {
        // killed a monster
        if (victim is Monster monster)
        {
            // gain exp if not in a party or if in a party without exp share
            if (!party.InParty() || !party.party.shareExperience)
                skillExperience += Experience.BalanceExperienceReward(monster.rewardSkillExperience, level.current, monster.level.current);
        }
    }
}
