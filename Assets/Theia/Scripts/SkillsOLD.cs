// skill system for all entities (players, monsters, npcs, towers, ...)
using System;
using UnityEngine;
using UnityEngine.Events;
using Mirror;

// serializable events
[Serializable] public class UnityEventSkill : UnityEvent<SkillOLD> {}

[DisallowMultipleComponent]
public abstract class SkillsOLD : NetworkBehaviour, IHealthBonus, IManaBonus, ICombatBonus
{
    [Header("Components")]
    public EntityOLD entity;
    public Health health;
    public ManaOLD mana;

    // 'skillTemplates' are the available skills (first one is default attack)
    // 'skills' are the loaded skills with cooldowns etc.
    [Header("Skills & Buffs")]
    public ScriptableSkill[] skillTemplates;
    public SyncList<SkillOLD> skills = new SyncList<SkillOLD>();
    public SyncList<Buff> buffs = new SyncList<Buff>(); // active buffs

    // effect mount is where the arrows/fireballs/etc. are spawned
    // -> can be overwritten, e.g. for mages to set it to the weapon's effect
    //    mount
    // -> assign to right hand if in doubt!
#pragma warning disable CS0649 // Field is never assigned to
    [SerializeField] Transform _effectMount;
#pragma warning restore CS0649 // Field is never assigned to
    public virtual Transform effectMount
    {
        get { return _effectMount; }
        set { _effectMount = value; }
    }

    [Header("Events")]
    public UnityEventSkill onSkillCastStarted;
    public UnityEventSkill onSkillCastFinished;

    // current skill (synced because we need it as an animation parameter)
    [SyncVar, HideInInspector] public int currentSkill = -1;

    // boni ////////////////////////////////////////////////////////////////////
    public int GetHealthBonus(int baseHealth)
    {
        // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
        int passiveBonus = 0;
        foreach (SkillOLD skill in skills)
            if (skill.level > 0 && skill.data is PassiveSkill passiveSkill)
                passiveBonus += passiveSkill.healthMaxBonus.Get(skill.level);

        int buffBonus = 0;
        foreach (Buff buff in buffs)
            buffBonus += buff.healthMaxBonus;

        return passiveBonus + buffBonus;
    }

    public int GetHealthRecoveryBonus()
    {
        // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
        float passivePercent = 0;
        foreach (SkillOLD skill in skills)
            if (skill.level > 0 && skill.data is PassiveSkill passiveSkill)
                passivePercent += passiveSkill.healthPercentPerSecondBonus.Get(skill.level);

        float buffPercent = 0;
        foreach (Buff buff in buffs)
            buffPercent += buff.healthPercentPerSecondBonus;

        return Convert.ToInt32(passivePercent * health.max) + Convert.ToInt32(buffPercent * health.max);
    }

    public int GetManaBonus(int baseMana)
    {
        // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
        int passiveBonus = 0;
        foreach (SkillOLD skill in skills)
            if (skill.level > 0 && skill.data is PassiveSkill passiveSkill)
                passiveBonus += passiveSkill.manaMaxBonus.Get(skill.level);

        int buffBonus = 0;
        foreach (Buff buff in buffs)
            buffBonus += buff.manaMaxBonus;

        return passiveBonus + buffBonus;
    }

    public int GetManaRecoveryBonus()
    {
        // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
        float passivePercent = 0;
        foreach (SkillOLD skill in skills)
            if (skill.level > 0 && skill.data is PassiveSkill passiveSkill)
                passivePercent += passiveSkill.manaPercentPerSecondBonus.Get(skill.level);

        float buffPercent = 0;
        foreach (Buff buff in buffs)
            buffPercent += buff.manaPercentPerSecondBonus;

        return Convert.ToInt32(passivePercent * mana.max) + Convert.ToInt32(buffPercent * mana.max);
    }

    public int GetDamageBonus()
    {
        // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
        int passiveBonus = 0;
        foreach (SkillOLD skill in skills)
            if (skill.level > 0 && skill.data is PassiveSkill passiveSkill)
                passiveBonus += passiveSkill.damageBonus.Get(skill.level);

        int buffBonus = 0;
        foreach (Buff buff in buffs)
            buffBonus += buff.damageBonus;

        return passiveBonus + buffBonus;
    }

    public int GetDefenseBonus()
    {
        // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
        int passiveBonus = 0;
        foreach (SkillOLD skill in skills)
            if (skill.level > 0 && skill.data is PassiveSkill passiveSkill)
                passiveBonus += passiveSkill.defenseBonus.Get(skill.level);

        int buffBonus = 0;
        foreach (Buff buff in buffs)
            buffBonus += buff.defenseBonus;

        return passiveBonus + buffBonus;
    }

    public float GetCriticalChanceBonus()
    {
        // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
        float passiveBonus = 0;
        foreach (SkillOLD skill in skills)
            if (skill.level > 0 && skill.data is PassiveSkill passiveSkill)
                passiveBonus += passiveSkill.criticalChanceBonus.Get(skill.level);

        float buffBonus = 0;
        foreach (Buff buff in buffs)
            buffBonus += buff.criticalChanceBonus;

        return passiveBonus + buffBonus;
    }

    public float GetBlockChanceBonus()
    {
        // sum up manually. Linq.Sum() is HEAVY(!) on GC and performance (190 KB/call!)
        float passiveBonus = 0;
        foreach (SkillOLD skill in skills)
            if (skill.level > 0 && skill.data is PassiveSkill passiveSkill)
                passiveBonus += passiveSkill.blockChanceBonus.Get(skill.level);

        float buffBonus = 0;
        foreach (Buff buff in buffs)
            buffBonus += buff.blockChanceBonus;

        return passiveBonus + buffBonus;
    }

    ////////////////////////////////////////////////////////////////////////////
    void Update()
    {
        // only update if it's worth updating (see IsWorthUpdating comments)
        if (isServer && entity.IsWorthUpdating())
            CleanupBuffs();
    }

    // helper function to find a skill index
    public int GetSkillIndexByName(string skillName)
    {
        // (avoid FindIndex to minimize allocations)
        for (int i = 0; i < skills.Count; ++i)
            if (skills[i].name == skillName)
                return i;
        return -1;
    }

    // helper function to find a buff index
    public int GetBuffIndexByName(string buffName)
    {
        // (avoid FindIndex to minimize allocations)
        for (int i = 0; i < buffs.Count; ++i)
            if (buffs[i].name == buffName)
                return i;
        return -1;
    }

    // the first check validates the caster
    // (the skill won't be ready if we check self while casting it. so the
    //  checkSkillReady variable can be used to ignore that if needed)
    // has a weapon (important for projectiles etc.), no cooldown, hp, mp?
    public bool CastCheckSelf(SkillOLD skill, bool checkSkillReady = true) =>
        skill.CheckSelf(entity, checkSkillReady);

    // the second check validates the target and corrects it for the skill if
    // necessary (e.g. when trying to heal an npc, it sets target to self first)
    // (skill shots that don't need a target will just return true if the user
    //  wants to cast them at a valid position)
    public bool CastCheckTarget(SkillOLD skill) =>
        skill.CheckTarget(entity);

    // the third check validates the distance between the caster and the target
    // (target entity or target position in case of skill shots)
    // note: castchecktarget already corrected the target (if any), so we don't
    //       have to worry about that anymore here
    public bool CastCheckDistance(SkillOLD skill, out Vector3 destination) =>
        skill.CheckDistance(entity, out destination);

    // starts casting
    [Server]
    public void StartCast(SkillOLD skill)
    {
        // start casting and set the casting end time
        skill.castTimeEnd = NetworkTime.time + skill.castTime;

        // save modifications
        skills[currentSkill] = skill;

        // rpc for client sided effects
        RpcCastStarted(skill);
    }

    // cancel a skill cast properly
    [Server]
    public void CancelCast(bool resetCurrentSkill = true)
    {
        // reset cast time, otherwise if a buff has a 10s cast time and we
        // cancel the cast after 1s, then we would have to wait 9 more seconds
        // before we can attempt to cast it again.
        // -> we cancel it in any case. players will have to wait for 'casttime'
        //    when attempting another cast anyway.
        if (currentSkill != -1)
        {
            SkillOLD skill = skills[currentSkill];
            skill.castTimeEnd = NetworkTime.time - skill.castTime;
            skills[currentSkill] = skill;

            // reset current skill
            if (resetCurrentSkill)
                currentSkill = -1;
        }
    }

    // finishes casting. casting and waiting has to be done in the state machine
    [Server]
    public void FinishCast(SkillOLD skill)
    {
        // * check if we can currently cast a skill (enough mana etc.)
        // * check if we can cast THAT skill on THAT target
        // note: we don't check the distance again. the skill will be cast even
        //   if the target walked a bit while we casted it (it's simply better
        //   gameplay and less frustrating)
        if (CastCheckSelf(skill, false) && CastCheckTarget(skill))
        {
            // let the skill template handle the action
            skill.Apply(entity);

            // rpc for client sided effects
            // -> pass that skill because skillIndex might be reset in the mean
            //    time, we never know
            RpcCastFinished(skill);

            // decrease mana in any case
            mana.current -= skill.manaCosts;

            // start the cooldown (and save it in the struct)
            skill.cooldownEnd = NetworkTime.time + skill.cooldown;

            // save any skill modifications in any case
            skills[currentSkill] = skill;
        }
        else
        {
            // not all requirements met. no need to cast the same skill again
            currentSkill = -1;
        }
    }

    // skill cast started rpc for client sided effects
    // note: pass Skill to avoid sync race conditions with indices etc.
    [ClientRpc]
    public void RpcCastStarted(SkillOLD skill)
    {
        // validate: still alive?
        if (health.current > 0)
        {
            // call scriptableskill event
            skill.data.OnCastStarted(entity);

            // call event
            onSkillCastStarted.Invoke(skill);
        }
    }

    // skill cast finished rpc for client sided effects
    // note: pass Skill to avoid sync race conditions with indices etc.
    [ClientRpc]
    public void RpcCastFinished(SkillOLD skill)
    {
        // validate: still alive?
        if (health.current > 0)
        {
            // call scriptableskill event
            skill.data.OnCastFinished(entity);

            // call event
            onSkillCastFinished.Invoke(skill);
        }
    }

    // helper function to add or refresh a buff
    public void AddOrRefreshBuff(Buff buff)
    {
        // reset if already in buffs list, otherwise add
        int index = GetBuffIndexByName(buff.name);
        if (index != -1) buffs[index] = buff;
        else buffs.Add(buff);
    }

    // helper function to remove all buffs that ended
    public void CleanupBuffs()
    {
        for (int i = 0; i < buffs.Count; ++i)
        {
            if (buffs[i].BuffTimeRemaining() == 0)
            {
                buffs.RemoveAt(i);
                --i;
            }
        }
    }

    [Server]
    public void OnDeath()
    {
        // clear buffs that shouldn't remain after death
        for (int i = 0; i < buffs.Count; ++i)
        {
            if (!buffs[i].remainAfterDeath)
            {
                buffs.RemoveAt(i);
                --i;
            }
        }

        // reset currently casted skill
        CancelCast();
    }
}
