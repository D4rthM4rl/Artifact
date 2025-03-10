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
    /// <summary>How long I have been regenerating mana</summary>
    protected float manaRegenTime = 0f;
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

    /// <summary>EffectTypes that are being applied when I attack </summary>
    private static List<EffectType> effectTypes = new List<EffectType>();
    /// <summary>What Effects my attacks inflict</summary>
    [System.NonSerialized]
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

    /// <summary>I take damage of a certain amount, taking defense into account</summary>
    /// <param name="damage">How much damage for me to take</param>
    public virtual void TakeDamage(float damage)
    {
        health = Mathf.Max(health - (damage / (defense * .1f)), 0);

        if (health <= 0) {Die();}
        UpdateHealth(health,maxHealth);
    }

    /// <summary>
    /// Taken damage for given amount considering or not considering defense amount
    /// </summary>
    /// <param name="damage">How much damage (pre-defense) for player to take</param>
    /// <param name="bypassDef">Whether attack should do given damage regardless of defense amount</param>
    public virtual void TakeDamage(float damage, bool bypassDef)
    {
        if (!bypassDef) health = Mathf.Max(health - Mathf.Max(damage - (defense * .1f), 0.001f), 0);
        else health = Mathf.Max(health - damage, 0);

        if (health <= 0) {Die();}
        UpdateHealth(health, maxHealth);
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
        StatChange move = effect.statChanges.moveSpeed;
        StatChange rate = effect.statChanges.attackRate;
        StatChange dam = effect.statChanges.attackDamage;
        StatChange size = effect.statChanges.attackSize;
        StatChange def = effect.statChanges.defense;
        // float currMove = MoveSpeed;
        // float currRate = AttackRateModifier;
        // float currDam = AttackDamageModifier;
        // float currSize = AttackSizeModifier;
        // float currDef = DefenseModifier;
        ChangeMoveSpeed(move.amount, move.multiplier);
        ChangeAttackRate(rate.amount, rate.multiplier);
        ChangeAttackDamage(dam.amount, dam.multiplier);
        ChangeAttackSize(size.amount, size.multiplier);
        ChangeDefense(def.amount, def.multiplier);
        if (effect.type == EffectType.radioactive) StartCoroutine(RadioactiveDamage(effect));
        else StartCoroutine(GradualHeal(-effect.damagePerHalfSec, 0.5f, effect.duration * 2));
        yield return new WaitForSeconds(effect.duration);
        Destroy(particleObject);
        afflictedBy.Remove(effect.type);
        ChangeMoveSpeed(1 / move.amount, move.multiplier);
        ChangeAttackRate(1 / rate.amount, rate.multiplier);
        ChangeAttackDamage(1 / dam.amount, dam.multiplier);
        ChangeAttackSize(1 / size.amount, size.multiplier);
        ChangeDefense(1 / def.amount, def.multiplier);
    }

    /// <summary>Removes amount of mana from bar</summary>
    /// <param name="amount">Amount of mana used</param>
    /// <returns>True if could use the mana, false otherwise</returns>
    public bool UseMana(float amount)
    {
        amount *= manaUseModifier;
        if (mana >= amount)
        {
            // timeSinceManaUsed = 0;
            mana -= amount;
            manaRegenTime = 0;
            UpdateMana(mana);
            return true;
        } else return false;
    }

    /// <summary>Changes how much mana every mana use takes by a modifier </summary>
    /// <param name="modifier">Amount to multiply modifier by</param>
    public void ReduceManaUsage(float modifier)
    {
        manaUseModifier *= modifier; 
    }

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
            health = (health - damage);
            UpdateHealth(health, maxHealth);
            if (health <= 0) {Die();}
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
            health = Mathf.Min(maxHealth, health + healAmount);
        }
        UpdateHealth(health, maxHealth);
    }

    /// <summary>Gradually heals me over time</summary>
    /// <param name="timePerHalfHeart">How long it takes to heal half a heart (sec)</param>
    /// <param name="healAmount">How much to heal total</param>
    protected virtual IEnumerator GradualHeal(float timePerHalfHeart, float healAmount)
    {
        health = Mathf.Min(maxHealth, health);
        for (int i = 0; i < healAmount - 1 ; i++) 
        {
            yield return new WaitForSeconds(timePerHalfHeart);
            health = Mathf.Clamp(health + 1, 0, maxHealth);
            if (health == 0) Die();
        }
        UpdateHealth(health, maxHealth);
    }

    /// <summary>Gradually heals me over time</summary>
    /// <param name="healAmount">How much to heal in each increment</param>
    /// <param name="healFrequency">How often to do a heal in sec</param>
    /// <param name="numHeals">How many heals to do</param>
    /// <returns></returns>
    protected virtual IEnumerator GradualHeal(float healAmount, float healFrequency, float numHeals)
    {
        health = Mathf.Min(maxHealth, health);
        for (int i = 0; i < numHeals - 1 ; i++) 
        {
            yield return new WaitForSeconds(healFrequency);
            health = Mathf.Clamp(health + healAmount, 0, maxHealth);
            if (health == 0) Die();
        }
        UpdateHealth(health, maxHealth);
    }
    
    /// <summary>Changes my max health</summary>
    /// <param name="healthChange">How much it should be changed</param>
    /// <param name="multiplier">Whether it should be multiplied</param>
    public virtual void ChangeMaxHealth(float healthChange, bool multiplier) // Max amount of hearts is set to 20 here.
    {
        if (multiplier) {
            maxHealth = (int)Mathf.Clamp(0, Mathf.Round(maxHealth * healthChange), 20);
        } else {
            maxHealth = (int) Mathf.Clamp(0, Mathf.Round(maxHealth + healthChange), 20); 
        }
        UpdateHealth(health, maxHealth);
    }

    /// <summary>Changes move speed</summary>
    /// <param name="moveSpeedChange">How much it should be changed</param>
    /// <param name="multiplier">Whether it should be multiplied</param>
    public void ChangeMoveSpeed(float moveSpeedChange, bool multiplier)
    {
        if (multiplier) {
            moveSpeed = Mathf.Max(0.5f, moveSpeed * moveSpeedChange);
        } else {
            moveSpeed = Mathf.Max(0.5f, moveSpeed + moveSpeedChange);
        }
    }

    /// <summary>Changes attack rate like swing rate or fire rate</summary>
    /// <param name="attackRateChange">How much it should be changed</param>
    /// <param name="multiplier">Whether it should be multiplied</param>
    public void ChangeAttackRate(float attackRateChange, bool multiplier)
    {
        if (multiplier) {
            attackRateModifier = Mathf.Max(0.01f, attackRateModifier * attackRateChange);
        } else {
            attackRateModifier = Mathf.Max(0.01f, attackRateModifier - attackRateChange);
        }
    }
    
    /// <summary>Changes attack damage</summary>
    /// <param name="attackDamageChange">How much it should be changed</param>
    /// <param name="multiplier">Whether it should be multiplied</param>
    public void ChangeAttackDamage(float attackDamageChange, bool multiplier)
    {
        if (multiplier) {
            attackDamageModifier = Mathf.Max(0.0001f, attackDamageModifier * attackDamageChange);
        } else {
            attackDamageModifier = Mathf.Max(0.0001f, attackDamageModifier + attackDamageChange);
        }
    }

    /// <summary>Changes defense to attacks so attacks do 1/(n*.1) times the damage</summary>
    /// <param name="defenseChange">How much it should be changed</param>
    /// <param name="multiplier">Whether it should be multiplied</param>
    public void ChangeDefense(float defenseChange, bool multiplier) {
        
        if (multiplier) {
            defense = Mathf.Max(0.01f, defense * defenseChange);
        } else {
            defense = Mathf.Max(0.01f, defense + defenseChange);
        }
    }

    // Changes Attack speed like bullet velocity or crit rate?
    // public void ChangeAttackSpeed(float attackSpeedChange, bool multiplier)
    // {
    //     if (multiplier) {
    //         attackRate = Mathf.Max(0.01f, attackRate * attackSpeedChange);
    //     } else {
    //         attackRate = Mathf.Max(0.01f, attackRate - attackSpeedChange);
    //     }
    // }

    /// <summary>Changes attack size</summary>
    /// <param name="attackSizeChange">How much it should be changed</param>
    /// <param name="multiplier">Whether it should be multiplied</param>
    public void ChangeAttackSize(float attackSizeChange, bool multiplier) 
    {
        if (multiplier) {
            attackSizeModifier = Mathf.Max(0f, attackSizeModifier * attackSizeChange);
        } else {
            attackSizeModifier = Mathf.Max(0f, attackSizeModifier + attackSizeChange);
        }
    }

    public abstract void Die();

    /// <summary>Does nothing for most characters unless stated otherwise</summary>
    /// <param name="mana">Mana amount to update to</param>
    protected virtual void UpdateMana(float mana) {}

    /// <summary>Increases mana by proper amount</summary>
    protected virtual void RegenMana() {
        if (mana < 100)
        {
            // if (timeSinceManaUsed >= .2f)
            // {
                mana = Mathf.Clamp(mana + Mathf.Pow(manaRegenTime, 1.3f) / 1 * manaRegenRate * Time.deltaTime, 0, 100);
                manaRegenTime += Time.deltaTime * 25;
                UpdateMana(mana);
            // } else timeSinceManaUsed += Time.deltaTime;
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
        float myScaledLevel = PowerLevel(health, maxHealth);
        float theirScaledLevel = other.PowerLevel(other.health, maxHealth);
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
        targetRb.AddForce(knockbackDirection * knockbackModifier, ForceMode.Impulse);
    }

    /// <summary>Returns power level of Character accounting for health</summary>
    /// <param name="health">Current amount of health</param>
    /// <param name="maxHealth">Amount of health possible for Character</param>
    /// <returns>The calculated power level accounting for health</returns>
    public virtual int PowerLevel(float health, float maxHealth)
        {return Mathf.RoundToInt(((health + 3) / (maxHealth + 3)) * powerLevel);}
}
