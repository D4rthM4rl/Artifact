using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// The state a Character is in: sleep being [not implemented], inactive being not pathfinding or attacking;
/// follow being following target but not attacking; attack being whatever attack pattern enemy has;
/// and die being destroying the enemy and dropping any item it has attached <br/>
/// For the player it is worked backwards from how they are controlling, like not moving for an extended
/// period of time puts them in sleep
/// </summary>
public enum CharacterState
{
    sleep,
    inactive,
    wander,
    follow,
    flee,
    attack,
    die,
};

/// <summary>What makes me flee, approach, and willing to attack</summary>
[System.Serializable]
public struct CharacterBehavior
{
    public List<WhenAttack> whenAttack;
    public List<WhenFollow> whenFollow;
    public List<WhenFlee> whenFlee;
    /// <summary>How close I will approach until I no longer move closer</summary>
    public float followDist;
    /// <summary>How close I will be to start fleeing</summary>
    public float fleeDist;
}

/// <summary>How I react to a specific other thing</summary>
[System.Serializable]
public struct Relationship
{
    /// <summary>What this Relationship is with</summary>
    public string name;
    /// <summary>How much it should prioritize this behavior</summary>
    public int priority;
    /// <summary>What ways to move</summary>
    public CharacterBehavior behavior;

    /// <summary>Creates a new Action about a focus</summary>
    /// <param name="other">What this Relationship is with</param>
    /// <param name="priority">How much it should prioritize this behavior</param>
    /// <param name="behavior">What ways to move</param>
    public Relationship(string other, int priority, CharacterBehavior behavior)
    {
        this.name = other;
        this.priority = priority;
        this.behavior = behavior;
    }
}

[System.Serializable]
public struct ItemRelationship
{
    /// <summary>What Item this Relationship is with</summary>
    public string name;
    /// <summary>How much it should prioritize this behavior</summary>
    public int priority;
    /// <summary>What ways to move</summary>
    public CharacterBehavior behavior;

    /// <summary>Creates a new Action about a focus</summary>
    /// <param name="other">What this Relationship is with</param>
    /// <param name="priority">How much it should prioritize this behavior</param>
    /// <param name="behavior">What ways to move</param>
    public ItemRelationship(string other, int priority, CharacterBehavior behavior)
    {
        this.name = other;
        this.priority = priority;
        this.behavior = behavior;
    }
}

/// <summary>
/// Action for a character to take. For example Flee from player with priority 1
/// </summary>
[System.Serializable]
public struct Action
{
    public MovementType movementType;
    public float distance;
    public int priority;
    public GameObject focus;

    /// <summary>Creates a new Action about a focus</summary>
    /// <param name="movementType">What way to move</param>
    /// <param name="priority">How much it should prioritize this action</param>
    /// <param name="focus">What this Action is relative to/focused on</param>
    public Action(MovementType movementType, int priority, GameObject focus, float distance)
    {
        this.movementType = movementType;
        this.distance = distance;
        this.priority = priority;
        this.focus = focus;
    }

    /// <summary>Creates a new Action about a focus</summary>
    /// <param name="movementType">What way to move, if Follow then then follow distance is 0</param>
    /// <param name="priority">How much it should prioritize this action</param>
    /// <param name="focus">What this Action is relative to/focused on</param>
    public Action(MovementType movementType, int priority, GameObject focus)
    {
        this.movementType = movementType;
        if (movementType == MovementType.Flee) this.distance = float.PositiveInfinity;
        else this.distance = 0;
        this.priority = priority;
        this.focus = focus;
    }
}

/// <summary>
/// Whether to move towards or away from something (roughly - ranged Characters would still be approaching)
/// </summary>
public enum MovementType
{
    Flee,
    Ignore,
    Follow
}

/// <summary>Stats for the character</summary>
[System.Serializable]
public class CharacterStats
{
    [Header("--------Character Stats--------")]
    /// <summary>How powerful character is, 1 being least powerful and 100 being most</summary>
    [Range(1, 100)]
    public int powerLevel;
    /// <summary>Current health</summary>
    public float health;
    /// <summary>Maximum amount of health possible from regen</summary>
    public float maxHealth;
    /// <summary>How fast I move</summary>
    [Range(0,100)]
    public float moveSpeed;
    /// <summary>How much mana (magic energy) I have left out of 100</summary>
    [Range(0,100)]
    public float mana = 100;
    // private static float timeSinceManaUsed = 1f;
    /// <summary>How much I use mana, 1 is normal amount, 2 is 2x mana cost</summary>
    public float manaUseModifier = 1f;
    /// <summary>How fast I regen mana</summary>
    public float manaRegenRate = 0.005f;
    /// <summary>How fast I attack</summary>
    public float attackRateModifier = 1f;
    /// <summary>How big my attacks are</summary>
    public float attackSizeModifier = 1f;
    /// <summary>How much damage I deal</summary>
    public float attackDamageModifier = 1f;
    /// <summary>How fast projectiles I shoot/spawn move</summary>
    public float projectileSpeedModifier = 1f;
    /// <summary>How long projectiles I shoot last</summary>
    public float projectileLifetimeModifier = 1f;
    /// <summary>How much knockback I deal</summary>
    public float knockbackModifier = 1f;
    /// <summary>How much less damage I take (roughly -.1 damage for +1 def)</summary>
    public float defense = 1f;

    public static readonly CharacterStats lowerBounds = new CharacterStats
    {
        powerLevel = 1,
        health = 0,
        maxHealth = 0,
        moveSpeed = 0.5f,
        mana = 0,
        manaUseModifier = 0.01f,
        manaRegenRate = 0f,
        attackRateModifier = 0.01f,
        attackSizeModifier = 0.1f,
        attackDamageModifier = 0.01f,
        projectileSpeedModifier = 0.01f,
        projectileLifetimeModifier = 0.1f,
        knockbackModifier = 0f,
        defense = 0.001f
    };

    public static readonly CharacterStats upperBounds = new CharacterStats
    {
        powerLevel = 100,
        health = 10000,
        maxHealth = 10000,
        moveSpeed = 100,
        mana = 100,
        manaUseModifier = 10000f,
        manaRegenRate = float.PositiveInfinity,
        attackRateModifier = 10000f,
        attackSizeModifier = 10000f,
        attackDamageModifier = float.PositiveInfinity,
        projectileSpeedModifier = 10000f,
        projectileLifetimeModifier = float.PositiveInfinity,
        knockbackModifier = float.PositiveInfinity,
        defense = float.PositiveInfinity
    };

    // A helper method to clone these stats
    public CharacterStats Clone()
    {
        return new CharacterStats
        {
            powerLevel = this.powerLevel,
            health = this.health,
            maxHealth = this.maxHealth,
            moveSpeed = this.moveSpeed,
            mana = this.mana,
            manaUseModifier = this.manaUseModifier,
            manaRegenRate = this.manaRegenRate,
            attackRateModifier = this.attackRateModifier,
            attackSizeModifier = this.attackSizeModifier,
            attackDamageModifier = this.attackDamageModifier,
            projectileSpeedModifier = this.projectileSpeedModifier,
            projectileLifetimeModifier = this.projectileLifetimeModifier,
            knockbackModifier = this.knockbackModifier,
            defense = this.defense
        };
    }
}

/// <summary>
/// Anything that can attack and be attacked, such as enemies and players and some summons
/// </summary>
public abstract class Character : MonoBehaviour
{
    /// <summary>Holds most of the info about the Character</summary>
    public CharacterInfo info;
    /// <summary>Where I hold weapons</summary>
    public Transform holdPoint;
    /// <summary>Stats of this character that are results of base stats and all changes</summary>
    [Tooltip("Actual current stats of the character")]
    public CharacterStats stats;

    /// <summary>
    /// List of currently applied stat changes.
    /// </summary>
    private List<StatChanges> appliedStatChanges = new List<StatChanges>();

    public int powerLevel;

    /// <summary>How long I have been regenerating mana</summary>
    private float manaRegenTime = 1f;

    

    /// <summary>EffectTypes that are being applied when I attack </summary>
    private static List<EffectType> effectTypes = new List<EffectType>();
    /// <summary>What Effects my attacks inflict</summary>
    public List<Effect> attackEffects = new List<Effect>();
    /// <summary>What Effects I'm afflicted by</summary>
    protected List<EffectType> afflictedBy = new List<EffectType>();
    /// <summary>What things (mostly Character but Item possible) I've been attacked by</summary>
    [System.NonSerialized]
    public List<string> attackedBy = new List<string>();
    /// <summary>Will attack this gameObject if I'm near enough</summary>
    public HashSet<GameObject> willAttack = new HashSet<GameObject>();

    /// <summary>State the enemy is in, starts wandering</summary>
    public CharacterState currState = CharacterState.wander;

    [HideInInspector]
    /// <summary>The prefab to spawn me again</summary>
    public GameObject prefab;

    protected virtual void Start() 
    {
        // Create a working copy of baseStats for final values?
        stats = info.BaseStats.Clone();
    }

    /// <summary>
    /// Adds a new stat change to the list and recalculates final stats.
    /// </summary>
    public void AddStatChange(StatChanges change)
    {
        appliedStatChanges.Add(change);
        RecalculateStats();
    }

    public IEnumerator AddStatChange(StatChanges change, float duration)
    {
        AddStatChange(change);
        if (duration == float.PositiveInfinity || duration <= 0) 
            Debug.LogError("Duration must >= 0 and less than infinity");
        yield return new WaitForSeconds(duration);
        RemoveStatChange(change);
    }

    /// <summary>
    /// Removes a stat change from the list and recalculates final stats.
    /// </summary>
    public void RemoveStatChange(StatChanges change)
    {
        Debug.Log(appliedStatChanges.Remove(change));
        RecalculateStats();
    }

    /// <summary>
    /// Recalculates finalStats by starting from baseStats and applying
    /// all changes—first additive, then multiplicative.
    /// </summary>
    protected void RecalculateStats()
    {
        // For each stat, we start with the base value,
        // add all additive modifications, then multiply by all multiplicative factors
        // Then we clamp to the stat upper and lower bounds
        stats.maxHealth = Mathf.Clamp(
            CalculateFinalStat(info.BaseStats.maxHealth, sc => sc.maxHealthChange), 
            CharacterStats.lowerBounds.maxHealth,
            CharacterStats.upperBounds.maxHealth);
        stats.moveSpeed = Mathf.Clamp(
            CalculateFinalStat(info.BaseStats.moveSpeed, sc => sc.moveSpeedChange), 
            CharacterStats.lowerBounds.moveSpeed,
            CharacterStats.upperBounds.moveSpeed);
        stats.attackDamageModifier = Mathf.Clamp(
            CalculateFinalStat(info.BaseStats.attackDamageModifier, sc => sc.attackDamageChange),
            CharacterStats.lowerBounds.attackDamageModifier,
            CharacterStats.upperBounds.attackDamageModifier);
        stats.attackRateModifier = Mathf.Clamp(
            CalculateFinalStat(info.BaseStats.attackRateModifier, sc => sc.attackRateChange),
            CharacterStats.lowerBounds.attackRateModifier,
            CharacterStats.upperBounds.attackRateModifier);
        stats.attackSizeModifier = Mathf.Clamp(
            CalculateFinalStat(info.BaseStats.attackSizeModifier, sc => sc.attackSizeChange),
            CharacterStats.lowerBounds.attackSizeModifier,
            CharacterStats.upperBounds.attackSizeModifier);
        stats.manaUseModifier = Mathf.Clamp(
            CalculateFinalStat(info.BaseStats.manaUseModifier, sc => sc.manaUseChange),
            CharacterStats.lowerBounds.manaUseModifier,
            CharacterStats.upperBounds.manaUseModifier);
        stats.manaRegenRate = Mathf.Clamp(
            CalculateFinalStat(info.BaseStats.manaRegenRate, sc => sc.manaRegenChange),
            CharacterStats.lowerBounds.manaRegenRate,
            CharacterStats.upperBounds.manaRegenRate);
        stats.projectileSpeedModifier = Mathf.Clamp(
            CalculateFinalStat(info.BaseStats.projectileSpeedModifier, sc => sc.projectileSpeedChange),
            CharacterStats.lowerBounds.projectileSpeedModifier,
            CharacterStats.upperBounds.projectileSpeedModifier);
        stats.projectileLifetimeModifier = Mathf.Clamp(
            CalculateFinalStat(info.BaseStats.projectileLifetimeModifier, sc => sc.projectileLifetimeChange),
            CharacterStats.lowerBounds.projectileLifetimeModifier,
            CharacterStats.upperBounds.projectileLifetimeModifier);
        stats.knockbackModifier = Mathf.Clamp(
            CalculateFinalStat(info.BaseStats.knockbackModifier, sc => sc.knockbackChange),
            CharacterStats.lowerBounds.knockbackModifier,
            CharacterStats.upperBounds.knockbackModifier);
        stats.defense = Mathf.Clamp(
            CalculateFinalStat(info.BaseStats.defense, sc => sc.defense),
            CharacterStats.lowerBounds.defense,
            CharacterStats.upperBounds.defense);


        stats.health = Mathf.Min(stats.health, stats.maxHealth);
        // Mana is clamped between 0 and 100:
        stats.mana = Mathf.Clamp(stats.mana, 0, 100);

        // Now update any UI or dependent systems with these new finalStats.
        UpdateHealth(stats.health, stats.maxHealth);
        UpdateMana(stats.mana);
    }

    /// <summary>
    /// Helper method that calculates the final value for a single stat.
    /// </summary>
    /// <param name="baseValue">The base value of the stat</param>
    /// <param name="selector">A function to select the StatChange for this stat from a StatChanges instance</param>
    /// <returns>The final stat value after all changes are applied</returns>
    private float CalculateFinalStat(float baseValue, System.Func<StatChanges, StatChange> selector)
    {
        // Sum all additive modifications (where multiplier is false)
        float additive = appliedStatChanges.Sum(sc => !selector(sc).multiplier ? selector(sc).amount : 0);
        // Multiply all multiplicative modifications (where multiplier is true)
        float multiplicative = appliedStatChanges.Aggregate(1f, (prod, sc) => selector(sc).multiplier ? prod * selector(sc).amount : prod);
        return (baseValue + additive) * multiplicative;
    }

    /// <summary>I take damage of a certain amount, taking defense into account</summary>
    /// <param name="damage">How much damage for me to take</param>
    public virtual void TakeDamage(float damage)
    {
        stats.health = Mathf.Max(stats.health - (damage / (stats.defense * .1f)), 0);

        if (stats.health <= 0) {Die();}
        UpdateHealth(stats.health, stats.maxHealth);
    }

    /// <summary>
    /// Taken damage for given amount considering or not considering defense amount
    /// </summary>
    /// <param name="damage">How much damage (pre-defense) for player to take</param>
    /// <param name="bypassDef">Whether attack should do given damage regardless of defense amount</param>
    public virtual void TakeDamage(float damage, bool bypassDef)
    {
        if (!bypassDef) stats.health = Mathf.Max(stats.health - Mathf.Max(damage - (stats.defense * .1f), 0.001f), 0);
        else stats.health = Mathf.Max(stats.health - damage, 0);

        if (stats.health <= 0) {Die();}
        UpdateHealth(stats.health, stats.maxHealth);
    }

    /// <summary>
    /// Gets an Effect applied asynchronously so effects don't die with bullet/attack GameObject
    /// </summary>
    /// <param name="e">Effect to be applied</param>
    public virtual void ReceiveEffect(Effect e) {
        StartCoroutine(ReceiveEffectAsync(e));
    }

    /// <summary>
    /// Gets an Effect applied with list of effects asynchronously so effects don't die with bullet/attack GameObject
    /// </summary>
    /// <param name="es">List of Effects to be have applied</param>
    public virtual void ReceiveEffect(List<Effect> es) 
    {
        foreach (Effect e in es)
        {
            StartCoroutine(ReceiveEffectAsync(EffectController.instance.GetEffect(e)));
        }
    }

    /// <summary>
    /// Applies debuff or buff Effect to player but if Effect has damage over time factor it's not applied
    /// because we don't want player to take damage over time
    /// </summary>
    /// <param name="effect">Effect to be applied</param>
    protected virtual IEnumerator ReceiveEffectAsync(Effect effect)
    {
        // Maybe resetTimer
        if (afflictedBy.Contains(effect.type)) {yield break;}
        afflictedBy.Add(effect.type);
        GameObject p = effect.particles;
        
        // Component particleComp = instance.gameObject.AddComponent(p.GetType());
        GameObject particleObject = Instantiate(p, transform);

        StartCoroutine(AddStatChange(effect.statChanges, effect.duration));
        if (effect.type == EffectType.radioactive) StartCoroutine(RadioactiveDamage(effect));
        else StartCoroutine(GradualHeal(-effect.damagePerHalfSec, 0.5f, effect.duration * 2));
        yield return new WaitForSeconds(effect.duration);
        Destroy(particleObject);
        afflictedBy.Remove(effect.type);
    }

    /// <summary>Removes amount of mana from bar</summary>
    /// <param name="amount">Amount of mana used</param>
    /// <returns>True if could use the mana, false otherwise</returns>
    public bool UseMana(float amount)
    {
        amount *= stats.manaUseModifier;
        if (stats.mana >= amount)
        {
            // timeSinceManaUsed = 0;
            stats.mana -= amount;
            manaRegenTime = 1;
            UpdateMana(stats.mana);
            return true;
        } else return false;
    }

    public abstract void Die();

    /// <summary>Does nothing for most characters unless stated otherwise</summary>
    /// <param name="mana">Mana amount to update to</param>
    protected virtual void UpdateMana(float mana) {}

    /// <summary>Increases mana by proper amount</summary>
    protected virtual void RegenMana() 
    {
        if (stats.mana < 100)
        {
            // Store the previous elapsed time
            float previousTime = manaRegenTime;

            // Increment elapsed time using the actual time passed
            manaRegenTime += Time.deltaTime;

            // Compute the exact amount regenerated over this frame
            float regenIncrement = (Mathf.Pow(manaRegenTime, 2.3f) - 
                Mathf.Pow(previousTime, 2.3f)) / 2.3f * (stats.manaRegenRate / 1);

            // Update mana, clamping to ensure it stays within bounds
            stats.mana = Mathf.Clamp(stats.mana + regenIncrement, 0, 100);
            UpdateMana(stats.mana);

        }
    }

    /// <summary>Does nothing for most characters unless stated otherwise</summary>
    /// <param name="health">Health amount to update to</param>
    /// <param name="maxHealth">Max health to update to</param>
    protected virtual void UpdateHealth(float health, float maxHealth) {}

    /// <summary>Returns whether I'm weaker than another Character</summary>
    /// <returns>True if my power level scaled with my health is lower than
    /// their power level scaled with their health</returns>
    protected virtual bool WeakerThan(Character other)
    {
        float myScaledLevel = PowerLevel(stats.health, stats.maxHealth);
        float theirScaledLevel = other.PowerLevel(other.stats.health, stats.maxHealth);
        return myScaledLevel < theirScaledLevel;
    }

    /// <summary>Hit a character</summary>
    /// <param name="recipient">Other Character to hit</param>
    /// <param name="damage">How much damage (not accounting for def etc.)
    /// for the other Character to take</param>
    public virtual void HitCharacter(Character recipient, float damage)
    {
        recipient.TakeDamage(damage, false);
        recipient.ReceiveEffect(attackEffects);
        recipient.attackedBy.Add(info.species); 
        Vector3 knockbackDirection = (recipient.transform.position - transform.position).normalized;
        if (recipient is Enemy) StartCoroutine((recipient as Enemy).Alert(recipient.gameObject));

        // Apply knockback force to what I'm hitting
        Rigidbody targetRb = recipient.gameObject.GetComponent<Rigidbody>();
        targetRb.AddForce(knockbackDirection * stats.knockbackModifier, ForceMode.Impulse);
    }

    /// <summary>Returns power level of Character accounting for health</summary>
    /// <param name="health">Current amount of health</param>
    /// <param name="maxHealth">Amount of health possible for Character</param>
    /// <returns>The calculated power level accounting for health</returns>
    public virtual int PowerLevel(float health, float maxHealth)
        {return Mathf.RoundToInt(((health + 3) / (maxHealth + 3)) * stats.powerLevel);}

    /// <summary>Adds new Effect to apply to my attacks</summary>
    /// <param name="e">Effect to apply to player's attacks</param>
    public void AddAttackEffect(Effect e)
    {
        if (!effectTypes.Contains(e.type))
        {
            attackEffects.Add(e);
        }
    }

    /// <summary>Takes exponential damage over time from radioactive effect</summary>
    /// <param name="eff">Specific Radioactive Effect to use</param>
    protected virtual IEnumerator RadioactiveDamage(Effect eff)
    {
        // health = Mathf.Min(health + 1);

        float timeElapsed = 0;
        while (timeElapsed < eff.duration) 
        {
            yield return new WaitForSeconds(0.01f);
            timeElapsed += 0.01f;
            // Logistic curve of the damage but divides by 100 at the end for a better number
            int d = Mathf.FloorToInt(eff.damagePerHalfSec * 0.1f); // Max damage on the curve per hit
            float k = 0.5f; // Damage curve slope (probably don't change)
            float x0 = 10 - eff.level; // Midpoint of damage curve
            float damage = d / (1 + Mathf.Pow(2.72f, (-k * (timeElapsed - x0)))); // Uses logistic curve
            stats.health = (stats.health - damage);
            UpdateHealth(stats.health, stats.maxHealth);
            if (stats.health <= 0) {Die();}
        }
    }

    /// <summary>Heals me</summary>
    /// <param name="healAmount">How much I should be healed</param>
    /// <param name="timeSpan">How long it takes to heal this amount</param>
    public virtual void Heal(float healAmount, float timeSpan)
    {
        if (timeSpan > 0) {
            float timePerHalfHeart = timeSpan / healAmount;
            StartCoroutine(GradualHeal(timePerHalfHeart, timeSpan));
        } else {
            stats.health = Mathf.Min(stats.maxHealth, stats.health + healAmount);
        }
        UpdateHealth(stats.health, stats.maxHealth);
    }

    /// <summary>Heals my mana</summary>
    /// <param name="healAmount">How much I should be healed</param>
    /// <param name="timeSpan">How long it takes to heal this amount</param>
    public virtual void HealMana(float healAmount, float timeSpan)
    {
        if (timeSpan > 0) {
            float timePerUnit = timeSpan / healAmount;
            StartCoroutine(GradualHealMana(timePerUnit, timeSpan));
        } else {
            stats.health = Mathf.Min(100, stats.mana + healAmount);
        }
        UpdateMana(stats.mana);
    }

    /// <summary>Gradually heals me over time</summary>
    /// <param name="timePerHalfHeart">How long it takes to heal half a heart (sec)</param>
    /// <param name="healAmount">How much to heal total</param>
    protected virtual IEnumerator GradualHeal(float timePerHalfHeart, float healAmount)
    {
        stats.health = Mathf.Min(stats.maxHealth, stats.health);
        for (int i = 0; i < healAmount - 1 ; i++) 
        {
            yield return new WaitForSeconds(timePerHalfHeart);
            stats.health = Mathf.Clamp(stats.health + 1, 0, stats.maxHealth);
            if (stats.health == 0) Die();
        }
        UpdateHealth(stats.health, stats.maxHealth);
    }

    /// <summary>Gradually heals my mana over time</summary>
    /// <param name="healAmount">How much to heal in each increment</param>
    /// <param name="healFrequency">How often to do a heal in sec</param>
    /// <param name="numHeals">How many heals to do</param>
    protected virtual IEnumerator GradualHeal(float healAmount, float healFrequency, float numHeals)
    {
        stats.health = Mathf.Min(stats.maxHealth, stats.health);
        for (int i = 0; i < numHeals - 1 ; i++) 
        {
            yield return new WaitForSeconds(healFrequency);
            stats.health = Mathf.Clamp(stats.health + healAmount, 0, stats.maxHealth);
            if (stats.health == 0) Die();
        }
        UpdateHealth(stats.health, stats.maxHealth);
    }
    
    /// <summary>Gradually heals my mana over time</summary>
    /// <param name="timePerUnit">How long it takes to heal half a heart (sec)</param>
    /// <param name="healAmount">How much to heal total</param>
    protected virtual IEnumerator GradualHealMana(float timePerUnit, float healAmount)
    {
        stats.mana = Mathf.Min(100, stats.mana);
        for (int i = 0; i < healAmount - 1 ; i++) 
        {
            yield return new WaitForSeconds(timePerUnit);
            stats.mana = Mathf.Clamp(stats.mana + 1, 0, 100);
        }
        UpdateMana(stats.mana);
    }

    /// <summary>Gradually heals my mana over time</summary>
    /// <param name="healAmount">How much to heal in each increment</param>
    /// <param name="healFrequency">How often to do a heal in sec</param>
    /// <param name="numHeals">How many heals to do</param>
    protected virtual IEnumerator GradualHealMana(float healAmount, float healFrequency, float numHeals)
    {
        stats.mana = Mathf.Min(100, stats.mana);
        for (int i = 0; i < numHeals - 1 ; i++) 
        {
            yield return new WaitForSeconds(healFrequency);
            stats.mana = Mathf.Clamp(stats.mana + healAmount, 0, 100);
        }
        UpdateMana(stats.mana);
    }
}
