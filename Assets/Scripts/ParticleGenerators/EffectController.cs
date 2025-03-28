using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Type of effect: burn, frost, stun, radioactive, poison, random, statChange <br/>
/// Burn: - Damage dealt, - Knockback dealt <br/>
/// Frost: - Movement speed, - Defense (+ Damage taken) <br/>
/// Stun: - Movement speed, - Attack Rate <br/>
/// Radioactive: + Damage dealt, - Defense (+ Damage taken) <br/>
/// Poison: - Attack Rate <br/>
/// </summary>
public enum EffectType {
    burn, frost, stun, radioactive, poison, random, statChange
}

// #nullable enable
/// <summary>
/// Effect to apply to enemies or players with certain characteristics that affect stats or deal damage/health over time
/// </summary>
[System.Serializable]
public class Effect
{
    /// <summary>
    /// GameObject which produces particles for that which is afflicted by the Effect
    /// </summary>
    public GameObject particles;
    /// <summary>
    /// Type of effect (EffectType.Burn, etc)
    /// </summary>
    public EffectType type;
    /// <summary>
    /// How strong effect is, <see cref="EffectController.GetStatChanges"/>
    /// </summary>
    public int level;
    /// <summary>
    /// How much damage per .5 sec
    /// </summary>
    [Tooltip("How much damage per .5 seconds")]
    public float damagePerHalfSec;
    /// <summary>
    /// How long effect lasts in sec
    /// </summary>
    [Tooltip("How long effect lasts in seconds")]
    public float duration;
    /// <summary>
    /// Stat Changes to apply with Effect
    /// </summary>
    public StatChanges statChanges;

    /// <summary>
    /// Constructs an Effect to apply
    /// </summary>
    /// <param name="e">Type of effect (EffectType.Burn, etc)</param>
    /// <param name="level">How strong effect is, <see cref="EffectController.GetStatChanges"/></param>
    /// <param name="damage">How much damage per .5 sec</param>
    /// <param name="duration">How long effect lasts in sec</param>
    public Effect(EffectType e, int level, float damage, float duration)
    {
        this.type = e;
        this.level = level;
        this.damagePerHalfSec = damage;
        this.duration = duration;
        this.particles = EffectController.instance.GetParticles(type, level);
        this.statChanges = EffectController.instance.GetStatChanges(type, level);
    }

    #nullable enable
    /// <summary>
    /// Constructs an Effect of stat change
    /// </summary>
    /// <param name="statChanges">What stats to change and how</param>
    /// <param name="duration">How long effect lasts in sec</param>
    public Effect(StatChanges? statChanges, float duration)
    {
        this.type = EffectType.statChange;
        this.level = 0;
        this.damagePerHalfSec = 0;
        this.duration = duration;
        this.particles = EffectController.instance.GetParticles(type, level);
        // Create a new StatChanges with non-nullable StatChange values
        if (statChanges == null) statChanges = new StatChanges();
    }
}

#nullable disable
public class EffectController : MonoBehaviour
{
    public static EffectController instance;
    public GameObject burn;
    public GameObject frost;
    public GameObject stun;
    public GameObject radioactive;
    public GameObject poison;
    public GameObject statChange;

    private void Awake()
    {
        if (instance == null) instance = this;
    }
    /// <summary>
    /// Gets an effect object from effect info
    /// </summary>
    /// <returns>An Effect object from the given info</returns>
    public Effect GetEffect(EffectType e, int level, float damagePerHalfSec, float duration)
    {
        return new Effect(e, level, damagePerHalfSec, duration);
    }

    /// <summary>
    /// Gets an effect object from a fake Effect object that doesn't work
    /// </summary>
    /// <param name="e">Effect object that doesn't work for some things</param>
    /// <returns></returns>
    public Effect GetEffect(Effect e)
    {
        return new Effect(e.type, e.level, e.damagePerHalfSec, e.duration);
    }

    /// <summary>
    /// Gets the type of particles that correspond to a type and level
    /// </summary>
    /// <param name="type">Type of Effect</param>
    /// <param name="level">Level of the Effect</param>
    /// <returns></returns>
    public GameObject GetParticles(EffectType type, int level) 
    {
        switch (type)
        {
            case(EffectType.burn): 
                return burn;
            case(EffectType.frost):
                return frost;
            case(EffectType.stun): 
                return stun;
            case(EffectType.radioactive):
                return radioactive;
            case(EffectType.poison): 
                return poison;
            case(EffectType.statChange):
                return statChange;
            default: 
                return statChange;
        }
    }

    /// <summary>
    /// Gets the StatChanges attached to a given Effect and its level
    /// </summary>
    /// <param name="type">Type of Effect</param>
    /// <param name="level">Level of Effect</param>
    /// <returns>6 StatChanges being moveSpeed, attackRate, attackSize, attackDamage, knockback, and defense</returns>
    public StatChanges GetStatChanges(EffectType type, int level)
    {
        StatChanges changes = new StatChanges();

        switch (type)
        {
            case(EffectType.burn): 
                changes.attackDamageChange.amount = 1 - 0.05f * level;
                changes.knockbackChange.amount = 1 - 0.05f * level;
                break;
            case(EffectType.frost):
                changes.moveSpeedChange.amount = 1 - 0.05f * level;
                changes.defense.amount = 1 - 0.05f * level;
                break;
            case(EffectType.stun):
                changes.moveSpeedChange.amount = 1 - 0.03f * level;
                changes.attackRateChange.amount = 1 + 0.03f * level;
                break;
            case(EffectType.radioactive):
                changes.attackDamageChange.amount = 1 + 0.05f * level;
                changes.defense.amount = 1 - 0.05f * level;
                break;
            case(EffectType.poison): 
                changes.attackRateChange.amount = 1 - 0.05f * level;
                changes.manaRegenChange.amount = 1 - 0.05f * level;
                break;
            case(EffectType.statChange):
                break;
        }

        return changes;
    }
}
