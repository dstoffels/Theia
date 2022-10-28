using System;
using UnityEngine;
using UnityEngine.Events;
using Mirror;

[RequireComponent(typeof(Level))]
[DisallowMultipleComponent]
public class Experience : NetworkBehaviour
{
    [Header("Components")]
    public Level level;

    [Header("Experience")] // note: int is not enough (can have > 2 mil. easily)
    [SyncVar, SerializeField] long _current = 0;
    public long current
    {
        get { return _current; }
        set
        {
            if (value <= _current)
            {
                // decrease
                _current = Math.Max(value, 0);
            }
            else
            {
                // increase with level ups
                // set the new value (which might be more than expMax)
                _current = value;

                // now see if we leveled up (possibly more than once too)
                // (can't level up if already max level)
                while (_current >= max && level.current < level.max)
                {
                    // subtract current level's required exp, then level up
                    _current -= max;
                    ++level.current;

                    // call event
                    onLevelUp.Invoke();
                }

                // set to expMax if there is still too much exp remaining
                if (_current > max) _current = max;
            }
        }
    }

    // required experience grows by 10% each level (like Runescape)
    [SerializeField] protected ExponentialLong _max = new ExponentialLong{multiplier=100, baseValue=1.1f};
    public long max { get { return _max.Get(level.current); } }

    [Header("Death")]
    public float deathLossPercent = 0.05f;

    [Header("Events")]
    public UnityEvent onLevelUp;

    // helper functions ////////////////////////////////////////////////////////
    public float Percent() =>
        (current != 0 && max != 0) ? (float)current / (float)max : 0;

    // players gain exp depending on their level. if a player has a lower level
    // than the monster, then he gains more exp (up to 100% more) and if he has
    // a higher level, then he gains less exp (up to 100% less)
    // -> see tests for several commented examples!
    public static long BalanceExperienceReward(long reward, int attackerLevel, int victimLevel, int maxLevelDifference = 20)
    {
        // level difference 10 means 10% extra/less per level.
        // level difference 20 means 5% extra/less per level.
        // so the percentage step depends on the level difference:
        float percentagePerLevel = 1f / maxLevelDifference;

        // calculate level difference. it should cap out at +- maxDifference to
        // avoid power level exploits where a level 1 player kills a level 100
        // monster and immediately gets millions of experience points and levels
        // up to level 50 instantly. this would be bad for MMOs.
        // instead, we only consider +- maxDifference.
        int levelDiff = Mathf.Clamp(victimLevel - attackerLevel, -maxLevelDifference, maxLevelDifference);

        // calculate the multiplier. it will be +10%, +20% etc. when killing
        // higher level monsters. it will be -10%, -20% etc. when killing lower
        // level monsters.
        float multiplier = 1 + levelDiff * percentagePerLevel;

        // calculate reward
        return Convert.ToInt64(reward * multiplier);
    }

    // events //////////////////////////////////////////////////////////////////
    [Server]
    public virtual void OnDeath()
    {
        // lose experience
        current -= Convert.ToInt64(max * deathLossPercent);
    }
}
