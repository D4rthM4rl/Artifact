using System.Collections;
using System.Collections.Generic;
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
}

/// <summary>
/// Anything that can attack and be attacked, such as enemies and players and some summons
/// </summary>
public abstract class Character : MonoBehaviour
{
    /// <summary>What type/species of character this is</summary>
    public string species;
    /// <summary>Brief description of the Character</summary>
    [TextArea(5,10)]
    public string description;
    /// <summary>Where I hold weapons</summary>
    public Transform holdPoint;
    /// <summary>Stats of this charaacter that aren't affected temporarily</summary>
    [Tooltip("Permanent stats of the character (not affected by temporary changes)")]
    public CharacterStats pStats;
    /// <summary>Stats of this character that do change from temporary changes</summary>
    private CharacterStats stats;

    public int powerLevel;

    /// <summary>How long I have been regenerating mana</summary>
    private float manaRegenTime = 1f;

    private bool useTempHealth = false;
    /// <summary>Current health</summary>
    public float Health
    {
        get {
            return useTempHealth ? stats.health : pStats.health;
        }
        set {
            if (useTempHealth)
            {
                stats.health = value;
            }
            else
            {
                pStats.health = value;
                stats.health = value;
            }
        }
    }

    private bool useTempMaxHealth = false;
    /// <summary>Maximum amount of health possible from regen</summary>
    public float MaxHealth
    {
        get {return useTempMaxHealth ? stats.maxHealth : pStats.maxHealth;}
        set {
            if (useTempMaxHealth) stats.maxHealth = value;
            else
            {
                pStats.maxHealth = value;
                stats.maxHealth = value;
            }
        }
    }

    private bool useTempMoveSpeed = false;
    /// <summary>How fast I move</summary>
    public float MoveSpeed 
    {
        get {return useTempMoveSpeed ? stats.moveSpeed : pStats.moveSpeed;}
        set {
            if (useTempMoveSpeed) stats.moveSpeed = value;
            else
            {
                pStats.moveSpeed = value;
                stats.moveSpeed = value;
            }
        }
    }

    private bool useTempMana = false;
    /// <summary>How much mana (magic energy) I have left out of 100</summary>
    public float Mana
    {
        get {return useTempMana ? stats.mana : pStats.mana;}
        set {
            if (useTempMana) stats.mana = value;
            else
            {
                pStats.mana = value;
                stats.mana = value;
            }
        }
    }
    
    // private static float timeSinceManaUsed = 1f;
    
    private bool useTempManaUseModifier = false;
    /// <summary>How much I use mana, 1 is normal amount, 2 is 2x mana cost</summary>
    public float ManaUseModifier
    {
        get {return useTempManaUseModifier ? stats.manaUseModifier : pStats.manaUseModifier;}
        set {
            if (useTempManaUseModifier) stats.manaUseModifier = value;
            else
            {
                pStats.manaUseModifier = value;
                stats.manaUseModifier = value;
            }
        }
    }

    private bool useTempManaRegenRate = false;
    /// <summary>How fast I regen mana</summary>
    public float ManaRegenRate
    {
        get {return useTempManaRegenRate ? stats.manaRegenRate : pStats.manaRegenRate;}
        set {
            if (useTempManaRegenRate) stats.manaRegenRate = value;
            else
            {
                pStats.manaRegenRate = value;
                stats.manaRegenRate = value;
            }
        }
    }

    private bool useTempAttackRateModifier = false;
    /// <summary>How fast I attack</summary>
    public float AttackRateModifier
    {
        get {return useTempAttackRateModifier ? stats.attackRateModifier : pStats.attackRateModifier;}
        set {
            if (useTempAttackRateModifier) stats.attackRateModifier = value;
            else
            {
                pStats.attackRateModifier = value;
                stats.attackRateModifier = value;
            }
        }
    }

    private bool useTempAttackSizeModifier = false;
    /// <summary>How big my attacks are</summary>
    public float AttackSizeModifier
    {
        get {return useTempAttackSizeModifier ? stats.attackSizeModifier : pStats.attackSizeModifier;}
        set {
            if (useTempAttackSizeModifier) stats.attackSizeModifier = value;
            else
            {
                pStats.attackSizeModifier = value;
                stats.attackSizeModifier = value;
            }
        }
    }

    private bool useTempAttackDamageModifier = false;
    /// <summary>How much damage I deal</summary>
    public float AttackDamageModifier
    {
        get {return useTempAttackDamageModifier ? stats.attackDamageModifier : pStats.attackDamageModifier;}
        set {
            if (useTempAttackDamageModifier) stats.attackDamageModifier = value;
            else
            {
                pStats.attackDamageModifier = value;
                stats.attackDamageModifier = value;
            }
        }
    }

    private bool useTempProjectileSpeedModifier = false;
    /// <summary>How fast projectiles I shoot/spawn move</summary>
    public float ProjectileSpeedModifier
    {
        get {return useTempProjectileSpeedModifier ? stats.projectileSpeedModifier : pStats.projectileSpeedModifier;}
        set {
            if (useTempProjectileSpeedModifier) stats.projectileSpeedModifier = value;
            else
            {
                pStats.projectileSpeedModifier = value;
                stats.projectileSpeedModifier = value;
            }
        }
    }

    private bool useTempProjectileLifetimeModifier = false;
    /// <summary>How long projectiles I shoot last</summary>
    public float ProjectileLifetimeModifier
    {
        get {return useTempProjectileLifetimeModifier ? stats.projectileLifetimeModifier : pStats.projectileLifetimeModifier;}
        set {
            if (useTempProjectileLifetimeModifier) stats.projectileLifetimeModifier = value;
            else
            {
                pStats.projectileLifetimeModifier = value;
                stats.projectileLifetimeModifier = value;
            }
        }
    }

    private bool useTempKnockbackModifier = false;
    /// <summary>How much knockback I deal</summary>
    public float KnockbackModifier
    {
        get {return useTempKnockbackModifier ? stats.knockbackModifier : pStats.knockbackModifier;}
        set {
            if (useTempKnockbackModifier) stats.knockbackModifier = value;
            else
            {
                pStats.knockbackModifier = value;
                stats.knockbackModifier = value;
            }
        }
    }

    private bool useTempDefense = false;
    /// <summary>How much less damage I take (roughly -.1 damage for +1 def)</summary>
    public float Defense
    {
        get {return useTempDefense ? stats.defense : pStats.defense;}
        set {
            if (useTempDefense) stats.defense = value;
            else
            {
                pStats.defense = value;
                stats.defense = value;
            }
        }
    }

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


    protected virtual void Start() 
    {
        stats = pStats;
    }

    /// <summary>I take damage of a certain amount, taking defense into account</summary>
    /// <param name="damage">How much damage for me to take</param>
    public virtual void TakeDamage(float damage)
    {
        Health = Mathf.Max(Health - (damage / (Defense * .1f)), 0);

        if (Health <= 0) {Die();}
        UpdateHealth(Health, MaxHealth);
    }

    /// <summary>
    /// Taken damage for given amount considering or not considering defense amount
    /// </summary>
    /// <param name="damage">How much damage (pre-defense) for player to take</param>
    /// <param name="bypassDef">Whether attack should do given damage regardless of defense amount</param>
    public virtual void TakeDamage(float damage, bool bypassDef)
    {
        if (!bypassDef) Health = Mathf.Max(Health - Mathf.Max(damage - (Defense * .1f), 0.001f), 0);
        else Health = Mathf.Max(Health - damage, 0);

        if (Health <= 0) {Die();}
        UpdateHealth(Health, MaxHealth);
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

        StatChanges.ChangeStats(this, effect.statChanges, effect.duration);
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
        amount *= ManaUseModifier;
        if (Mana >= amount)
        {
            // timeSinceManaUsed = 0;
            Mana -= amount;
            manaRegenTime = 1;
            UpdateMana(pStats.mana);
            return true;
        } else return false;
    }

    public abstract void Die();

    /// <summary>Does nothing for most characters unless stated otherwise</summary>
    /// <param name="mana">Mana amount to update to</param>
    protected virtual void UpdateMana(float mana) {}

    /// <summary>Increases mana by proper amount</summary>
    protected virtual void RegenMana() {
        if (Mana < 100)
        {
            // Store the previous elapsed time
            float previousTime = manaRegenTime;

            // Increment elapsed time using the actual time passed
            manaRegenTime += Time.deltaTime;

            // Compute the exact amount regenerated over this frame
            float regenIncrement = (Mathf.Pow(manaRegenTime, 2.3f) - Mathf.Pow(previousTime, 2.3f)) / 2.3f * (ManaRegenRate / 1);

            // Update mana, clamping to ensure it stays within bounds
            Mana = Mathf.Clamp(Mana + regenIncrement, 0, 100);
            UpdateMana(Mana);

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
        float myScaledLevel = PowerLevel(pStats.health, pStats.maxHealth);
        float theirScaledLevel = other.PowerLevel(other.pStats.health, pStats.maxHealth);
        return myScaledLevel < theirScaledLevel;
    }

    /// <summary>Hit a character</summary>
    /// <param name="other">Other Character to hit</param>
    /// <param name="damage">How much damage (not accounting for def etc.)
    /// the other Character to take</param>
    public virtual void HitCharacter(Character other, float damage)
    {
        other.TakeDamage(damage, false);
        other.ReceiveEffect(other.attackEffects);
        other.attackedBy.Add(other.species); 
        Vector3 knockbackDirection = (other.transform.position - transform.position).normalized;
        if (other is Enemy) StartCoroutine((other as Enemy).Alert(other.gameObject));

        // Apply knockback force to what I'm hitting
        Rigidbody targetRb = other.gameObject.GetComponent<Rigidbody>();
        targetRb.AddForce(knockbackDirection * pStats.knockbackModifier, ForceMode.Impulse);
    }

    /// <summary>Returns power level of Character accounting for health</summary>
    /// <param name="health">Current amount of health</param>
    /// <param name="maxHealth">Amount of health possible for Character</param>
    /// <returns>The calculated power level accounting for health</returns>
    public virtual int PowerLevel(float health, float maxHealth)
        {return Mathf.RoundToInt(((health + 3) / (maxHealth + 3)) * pStats.powerLevel);}

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
            pStats.health = (pStats.health - damage);
            UpdateHealth(pStats.health, pStats.maxHealth);
            if (pStats.health <= 0) {Die();}
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
            pStats.health = Mathf.Min(pStats.maxHealth, pStats.health + healAmount);
        }
        UpdateHealth(pStats.health, pStats.maxHealth);
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
            pStats.health = Mathf.Min(100, pStats.mana + healAmount);
        }
        UpdateMana(pStats.mana);
    }

    /// <summary>Gradually heals me over time</summary>
    /// <param name="timePerHalfHeart">How long it takes to heal half a heart (sec)</param>
    /// <param name="healAmount">How much to heal total</param>
    protected virtual IEnumerator GradualHeal(float timePerHalfHeart, float healAmount)
    {
        pStats.health = Mathf.Min(pStats.maxHealth, pStats.health);
        for (int i = 0; i < healAmount - 1 ; i++) 
        {
            yield return new WaitForSeconds(timePerHalfHeart);
            pStats.health = Mathf.Clamp(pStats.health + 1, 0, pStats.maxHealth);
            if (pStats.health == 0) Die();
        }
        UpdateHealth(pStats.health, pStats.maxHealth);
    }

    /// <summary>Gradually heals my mana over time</summary>
    /// <param name="healAmount">How much to heal in each increment</param>
    /// <param name="healFrequency">How often to do a heal in sec</param>
    /// <param name="numHeals">How many heals to do</param>
    protected virtual IEnumerator GradualHeal(float healAmount, float healFrequency, float numHeals)
    {
        pStats.health = Mathf.Min(pStats.maxHealth, pStats.health);
        for (int i = 0; i < numHeals - 1 ; i++) 
        {
            yield return new WaitForSeconds(healFrequency);
            pStats.health = Mathf.Clamp(pStats.health + healAmount, 0, pStats.maxHealth);
            if (pStats.health == 0) Die();
        }
        UpdateHealth(pStats.health, pStats.maxHealth);
    }
    
    /// <summary>Gradually heals my mana over time</summary>
    /// <param name="timePerUnit">How long it takes to heal half a heart (sec)</param>
    /// <param name="healAmount">How much to heal total</param>
    protected virtual IEnumerator GradualHealMana(float timePerUnit, float healAmount)
    {
        pStats.mana = Mathf.Min(100, pStats.mana);
        for (int i = 0; i < healAmount - 1 ; i++) 
        {
            yield return new WaitForSeconds(timePerUnit);
            pStats.mana = Mathf.Clamp(pStats.mana + 1, 0, 100);
        }
        UpdateMana(pStats.mana);
    }

    /// <summary>Gradually heals my mana over time</summary>
    /// <param name="healAmount">How much to heal in each increment</param>
    /// <param name="healFrequency">How often to do a heal in sec</param>
    /// <param name="numHeals">How many heals to do</param>
    protected virtual IEnumerator GradualHealMana(float healAmount, float healFrequency, float numHeals)
    {
        pStats.mana = Mathf.Min(100, pStats.mana);
        for (int i = 0; i < numHeals - 1 ; i++) 
        {
            yield return new WaitForSeconds(healFrequency);
            pStats.mana = Mathf.Clamp(pStats.mana + healAmount, 0, 100);
        }
        UpdateMana(pStats.mana);
    }



    /** PERMANENT STAT CHANGES
     * These are stat changes that are reverted to when temporary changes are done
     */

    /// <summary>Changes my max health</summary>
    /// <param name="healthChange">How much it should be changed</param>
    /// <param name="multiplier">Whether it should be multiplied</param>
    public virtual void ChangeMaxHealth(float healthChange, bool multiplier) // Max amount of hearts is set to 20 here.
    {
        if (multiplier) {
            pStats.maxHealth = (int)Mathf.Clamp(0, Mathf.Round(pStats.maxHealth * healthChange), 20);
        } else {
            pStats.maxHealth = (int) Mathf.Clamp(0, Mathf.Round(pStats.maxHealth + healthChange), 20); 
        }
        UpdateHealth(pStats.health, pStats.maxHealth);
    }

    /// <summary>Changes move speed</summary>
    /// <param name="moveSpeedChange">How much it should be changed</param>
    /// <param name="multiplier">Whether it should be multiplied</param>
    public void ChangeMoveSpeed(float moveSpeedChange, bool multiplier)
    {
        if (multiplier) {
            pStats.moveSpeed = Mathf.Max(0.5f, pStats.moveSpeed * moveSpeedChange);
        } else {
            pStats.moveSpeed = Mathf.Max(0.5f, pStats.moveSpeed + moveSpeedChange);
        }
    }

    /// <summary>Changes attack rate like swing rate or fire rate</summary>
    /// <param name="attackRateChange">How much it should be changed</param>
    /// <param name="multiplier">Whether it should be multiplied</param>
    public void ChangeAttackRate(float attackRateChange, bool multiplier)
    {
        if (multiplier) {
            pStats.attackRateModifier = Mathf.Max(0.01f, pStats.attackRateModifier * attackRateChange);
        } else {
            pStats.attackRateModifier = Mathf.Max(0.01f, pStats.attackRateModifier - attackRateChange);
        }
    }
    
    /// <summary>Changes attack damage</summary>
    /// <param name="attackDamageChange">How much it should be changed</param>
    /// <param name="multiplier">Whether it should be multiplied</param>
    public void ChangeAttackDamage(float attackDamageChange, bool multiplier)
    {
        if (multiplier) {
            pStats.attackDamageModifier = Mathf.Max(0.0001f, pStats.attackDamageModifier * attackDamageChange);
        } else {
            pStats.attackDamageModifier = Mathf.Max(0.0001f, pStats.attackDamageModifier + attackDamageChange);
        }
    }

    /// <summary>Changes defense to attacks so attacks do 1/(n*.1) times the damage</summary>
    /// <param name="defenseChange">How much it should be changed</param>
    /// <param name="multiplier">Whether it should be multiplied</param>
    public void ChangeDefense(float defenseChange, bool multiplier) {
        
        if (multiplier) {
            pStats.defense = Mathf.Max(0.01f, pStats.defense * defenseChange);
        } else {
            pStats.defense = Mathf.Max(0.01f, pStats.defense + defenseChange);
        }
    }

    /// <summary>Changes attack size</summary>
    /// <param name="attackSizeChange">How much it should be changed</param>
    /// <param name="multiplier">Whether it should be multiplied</param>
    public void ChangeAttackSize(float attackSizeChange, bool multiplier) 
    {
        if (multiplier) {
            pStats.attackSizeModifier = Mathf.Max(0f, pStats.attackSizeModifier * attackSizeChange);
        } else {
            pStats.attackSizeModifier = Mathf.Max(0f, pStats.attackSizeModifier + attackSizeChange);
        }
    }

    /// <summary>Changes how much mana I use</summary>
    /// <param name="manaUseChange">How much it should be changed</param>
    /// <param name="multiplier">Whether it should be multiplied</param>
    public void ChangeManaUse(float manaUseChange, bool multiplier) 
    {
        if (multiplier) {
            pStats.manaUseModifier = Mathf.Max(0f, pStats.manaUseModifier * manaUseChange);
        } else {
            pStats.manaUseModifier = Mathf.Max(0f, pStats.manaUseModifier + manaUseChange);
        }
    }

    /// <summary>Changes how fast I regenerate mana</summary>
    /// <param name="manaRegenChange">How much it should be changed</param>
    /// <param name="multiplier">Whether it should be multiplied</param>
    public void ChangeManaRegen(float manaRegenChange, bool multiplier) 
    {
        if (multiplier) {
            pStats.manaRegenRate = Mathf.Max(0f, pStats.manaRegenRate * manaRegenChange);
        } else {
            pStats.manaRegenRate = Mathf.Max(0f, pStats.manaRegenRate + manaRegenChange);
        }
    }

    /// <summary>Changes how fast my projectiles move</summary>
    /// <param name="projectileSpeedChange">How much it should be changed</param>
    /// <param name="multiplier">Whether it should be multiplied</param>
    public void ChangeProjectileSpeed(float projectileSpeedChange, bool multiplier) 
    {
        if (multiplier) {
            pStats.projectileSpeedModifier = Mathf.Max(0f, pStats.projectileSpeedModifier * projectileSpeedChange);
        } else {
            pStats.projectileSpeedModifier = Mathf.Max(0f, pStats.projectileSpeedModifier + projectileSpeedChange);
        }
    }

    /// <summary>Changes how long my projectiles last</summary>
    /// <param name="projectileLifetimeChange">How much it should be changed</param>
    /// <param name="multiplier">Whether it should be multiplied</param>
    public void ChangeProjectileLifetime(float projectileLifetimeChange, bool multiplier) 
    {
        if (multiplier) {
            pStats.projectileLifetimeModifier = Mathf.Max(0f, pStats.projectileLifetimeModifier * projectileLifetimeChange);
        } else {
            pStats.projectileLifetimeModifier = Mathf.Max(0f, pStats.projectileLifetimeModifier + projectileLifetimeChange);
        }
    }

    /// <summary>Changes how much my attacks knock others back</summary>
    /// <param name="knockbackChange">How much it should be changed</param>
    /// <param name="multiplier">Whether it should be multiplied</param>
    public void ChangeKnockback(float knockbackChange, bool multiplier) 
    {
        if (multiplier) {
            pStats.knockbackModifier = Mathf.Max(0f, pStats.knockbackModifier * knockbackChange);
        } else {
            pStats.knockbackModifier = Mathf.Max(0f, pStats.knockbackModifier + knockbackChange);
        }
    }
    


    /** TEMPORARY STAT CHANGES
     * These are stat changes that are temporary and will revert back to normal after a certain 
     * amount of time or when something is unequipped
     */
    
    /// <summary>Changes my max health</summary>
    /// <param name="healthChange">How much it should be changed</param>
    /// <param name="multiplier">Whether it should be multiplied</param>
    public virtual void ChangeMaxHealthTemp(float healthChange, bool multiplier) // Max amount of health is set to 20 here.
    {
        useTempMaxHealth = true;
        if (multiplier) {
            stats.maxHealth = (int)Mathf.Clamp(0, Mathf.Round(stats.maxHealth * healthChange), 20);
        } else {
            stats.maxHealth = (int) Mathf.Clamp(0, Mathf.Round(stats.maxHealth + healthChange), 20); 
        }
        UpdateHealth(stats.health, stats.maxHealth);
    }

    /// <summary>Changes move speed</summary>
    /// <param name="moveSpeedChange">How much it should be changed</param>
    /// <param name="multiplier">Whether it should be multiplied</param>
    public void ChangeMoveSpeedTemp(float moveSpeedChange, bool multiplier)
    {
        useTempMoveSpeed = true;
        if (multiplier) {
            stats.moveSpeed = Mathf.Max(0.5f, stats.moveSpeed * moveSpeedChange);
        } else {
            stats.moveSpeed = Mathf.Max(0.5f, stats.moveSpeed + moveSpeedChange);
        }
    }

    /// <summary>Changes attack rate like swing rate or fire rate</summary>
    /// <param name="attackRateChange">How much it should be changed</param>
    /// <param name="multiplier">Whether it should be multiplied</param>
    public void ChangeAttackRateTemp(float attackRateChange, bool multiplier)
    {
        useTempAttackRateModifier = true;
        if (multiplier) {
            stats.attackRateModifier = Mathf.Max(0.01f, stats.attackRateModifier * attackRateChange);
        } else {
            stats.attackRateModifier = Mathf.Max(0.01f, stats.attackRateModifier - attackRateChange);
        }
    }
    
    /// <summary>Changes attack damage</summary>
    /// <param name="attackDamageChange">How much it should be changed</param>
    /// <param name="multiplier">Whether it should be multiplied</param>
    public void ChangeAttackDamageTemp(float attackDamageChange, bool multiplier)
    {
        useTempAttackDamageModifier = true;
        if (multiplier) {
            stats.attackDamageModifier = Mathf.Max(0.0001f, stats.attackDamageModifier * attackDamageChange);
        } else {
            stats.attackDamageModifier = Mathf.Max(0.0001f, stats.attackDamageModifier + attackDamageChange);
        }
    }

    /// <summary>Changes defense to attacks so attacks do 1/(n*.1) times the damage</summary>
    /// <param name="defenseChange">How much it should be changed</param>
    /// <param name="multiplier">Whether it should be multiplied</param>
    public void ChangeDefenseTemp(float defenseChange, bool multiplier) 
    {
        useTempDefense = true;
        if (multiplier) {
            stats.defense = Mathf.Max(0.01f, stats.defense * defenseChange);
        } else {
            stats.defense = Mathf.Max(0.01f, stats.defense + defenseChange);
        }
    }

    /// <summary>Changes attack size</summary>
    /// <param name="attackSizeChange">How much it should be changed</param>
    /// <param name="multiplier">Whether it should be multiplied</param>
    public void ChangeAttackSizeTemp(float attackSizeChange, bool multiplier) 
    {
        useTempAttackSizeModifier = true;
        if (multiplier) {
            stats.attackSizeModifier = Mathf.Max(0f, stats.attackSizeModifier * attackSizeChange);
        } else {
            stats.attackSizeModifier = Mathf.Max(0f, stats.attackSizeModifier + attackSizeChange);
        }
    }

    /// <summary>Changes how much mana I use</summary>
    /// <param name="manaUseChange">How much it should be changed</param>
    /// <param name="multiplier">Whether it should be multiplied</param>
    public void ChangeManaUseTemp(float manaUseChange, bool multiplier) 
    {
        useTempManaUseModifier = true;
        if (multiplier) {
            stats.manaUseModifier = Mathf.Max(0f, pStats.manaUseModifier * manaUseChange);
        } else {
            stats.manaUseModifier = Mathf.Max(0f, pStats.manaUseModifier + manaUseChange);
        }
    }

    /// <summary>Changes how fast I regenerate mana</summary>
    /// <param name="manaRegenChange">How much it should be changed</param>
    /// <param name="multiplier">Whether it should be multiplied</param>
    public void ChangeManaRegenTemp(float manaRegenChange, bool multiplier) 
    {
        useTempManaRegenRate = true;
        if (multiplier) {
            stats.manaRegenRate = Mathf.Max(0f, stats.manaRegenRate * manaRegenChange);
        } else {
            stats.manaRegenRate = Mathf.Max(0f, stats.manaRegenRate + manaRegenChange);
        }
    }

    /// <summary>Changes how fast my projectiles move</summary>
    /// <param name="projectileSpeedChange">How much it should be changed</param>
    /// <param name="multiplier">Whether it should be multiplied</param>
    public void ChangeProjectileSpeedTemp(float projectileSpeedChange, bool multiplier) 
    {
        useTempProjectileSpeedModifier = true;
        if (multiplier) {
            stats.projectileSpeedModifier = Mathf.Max(0f, stats.projectileSpeedModifier * projectileSpeedChange);
        } else {
            stats.projectileSpeedModifier = Mathf.Max(0f, stats.projectileSpeedModifier + projectileSpeedChange);
        }
    }

    /// <summary>Changes how long my projectiles last</summary>
    /// <param name="projectileLifetimeChange">How much it should be changed</param>
    /// <param name="multiplier">Whether it should be multiplied</param>
    public void ChangeProjectileLifetimeTemp(float projectileLifetimeChange, bool multiplier) 
    {
        useTempProjectileLifetimeModifier = true;
        if (multiplier) {
            stats.projectileLifetimeModifier = Mathf.Max(0f, stats.projectileLifetimeModifier * projectileLifetimeChange);
        } else {
            stats.projectileLifetimeModifier = Mathf.Max(0f, stats.projectileLifetimeModifier + projectileLifetimeChange);
        }
    }

    /// <summary>Changes how much my attacks knock others back</summary>
    /// <param name="knockbackChange">How much it should be changed</param>
    /// <param name="multiplier">Whether it should be multiplied</param>
    public void ChangeKnockbackTemp(float knockbackChange, bool multiplier) 
    {
        useTempKnockbackModifier = true;
        if (multiplier) {
            stats.knockbackModifier = Mathf.Max(0f, stats.knockbackModifier * knockbackChange);
        } else {
            stats.knockbackModifier = Mathf.Max(0f, stats.knockbackModifier + knockbackChange);
        }
    }

    /// <summary>Sets the temporary stats back to the permanent ones</summary>
    public void ResetTempStats()
    {
        stats = pStats;
        useTempMaxHealth = false;
        useTempMoveSpeed = false;
        useTempAttackRateModifier = false;
        useTempAttackDamageModifier = false;
        useTempDefense = false;
        useTempAttackSizeModifier = false;
        useTempManaUseModifier = false;
        useTempManaRegenRate = false;
        useTempProjectileSpeedModifier = false;
        useTempProjectileLifetimeModifier = false;
        useTempKnockbackModifier = false;
        useTempProjectileSpeedModifier = false;
    }
}
