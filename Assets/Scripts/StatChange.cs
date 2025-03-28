using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StatChange
{
    private static StatChange _same = new StatChange(0, false);
    /// <summary>A StatChange which doesn't modify the stat</summary>
    public static StatChange same { get;} = _same;
    public float amount;
    public bool multiplier;

    public StatChange(float amount, bool multiplier)
    {
        this.amount = amount;
        this.multiplier = multiplier;
    }
}

[System.Serializable]
public class StatChanges 
{
    public Heal heal = new Heal(0, 0);
    public StatChange maxHealthChange = new StatChange(0, false);
    public StatChange moveSpeedChange = new StatChange(0, false);
    public StatChange attackDamageChange = new StatChange(0, false);
    public StatChange attackRateChange = new StatChange(0, false);
    public StatChange attackSizeChange = new StatChange(0, false);
    public ManaHeal manaHeal = new ManaHeal(0, 0);
    public StatChange manaUseChange = new StatChange(0, false);
    public StatChange manaRegenChange = new StatChange(0, false);
    public StatChange projectileSpeedChange = new StatChange(0, false);
    public StatChange projectileLifetimeChange = new StatChange(0, false);
    public StatChange knockbackChange = new StatChange(0, false);
    public StatChange defense = new StatChange(0, false);

    /// <summary>
    /// Change the stats of a character
    /// </summary>
    /// <param name="c">Character to change the stats of</param>
    /// <param name="s">StatChanges to apply</param>
    public static void ChangeStats(Character c, StatChanges s)
    {
        c.ChangeMaxHealth(s.maxHealthChange.amount, s.maxHealthChange.multiplier);

        c.Heal(s.heal.amount, s.heal.timeSpan);

        c.ChangeMoveSpeed(s.moveSpeedChange.amount, s.moveSpeedChange.multiplier);
        c.ChangeAttackDamage(s.attackDamageChange.amount, s.attackDamageChange.multiplier);
        c.ChangeAttackRate(s.attackRateChange.amount, s.attackRateChange.multiplier);
        c.ChangeAttackSize(s.attackSizeChange.amount, s.attackSizeChange.multiplier);

        c.HealMana(s.manaHeal.amount, s.manaHeal.timeSpan);

        c.ChangeManaUse(s.manaUseChange.amount, s.manaUseChange.multiplier);
        c.ChangeManaRegen(s.manaRegenChange.amount, s.manaRegenChange.multiplier);
        c.ChangeProjectileSpeed(s.projectileSpeedChange.amount, s.projectileSpeedChange.multiplier);
        c.ChangeProjectileLifetime(s.projectileLifetimeChange.amount, s.projectileLifetimeChange.multiplier);
        c.ChangeKnockback(s.knockbackChange.amount, s.knockbackChange.multiplier);
        c.ChangeDefense(s.defense.amount, s.defense.multiplier);
    }

    /// <summary>Change the stats of a Character temporarily</summary>
    /// <param name="c">Character to change the stats of</param>
    /// <param name="s">StatChanges to apply temporarily</param>
    /// <param name="duration">How long for the changes to last</param>
    public static IEnumerator ChangeStats(Character c, StatChanges s, float duration)
    {
        if (s.maxHealthChange != StatChange.same) 
        {
            c.ChangeMaxHealthTemp(s.maxHealthChange.amount, s.maxHealthChange.multiplier);
        }
        c.Heal(s.heal.amount, s.heal.timeSpan);

        c.ChangeMoveSpeedTemp(s.moveSpeedChange.amount, s.moveSpeedChange.multiplier);
        c.ChangeAttackDamageTemp(s.attackDamageChange.amount, s.attackDamageChange.multiplier);
        c.ChangeAttackRateTemp(s.attackRateChange.amount, s.attackRateChange.multiplier);
        c.ChangeAttackSizeTemp(s.attackSizeChange.amount, s.attackSizeChange.multiplier);

        c.HealMana(s.manaHeal.amount, s.manaHeal.timeSpan);

        c.ChangeManaUseTemp(s.manaUseChange.amount, s.manaUseChange.multiplier);
        c.ChangeManaRegenTemp(s.manaRegenChange.amount, s.manaRegenChange.multiplier);
        c.ChangeProjectileSpeedTemp(s.projectileSpeedChange.amount, s.projectileSpeedChange.multiplier);
        c.ChangeProjectileLifetimeTemp(s.projectileLifetimeChange.amount, s.projectileLifetimeChange.multiplier);
        c.ChangeKnockbackTemp(s.knockbackChange.amount, s.knockbackChange.multiplier);
        c.ChangeDefenseTemp(s.defense.amount, s.defense.multiplier);

        if (duration != float.PositiveInfinity)
        {
            yield return new WaitForSeconds(duration);
            c.ResetTempStats();
        }
    }

    // public IEnumerator<StatChange> GetEnumerator()
    // {
    //     yield return maxHealthChange;
    //     yield return moveSpeedChange;
    //     yield return attackDamageChange;
    //     yield return attackRateChange;
    //     yield return attackSizeChange;
    //     yield return manaUseChange;
    //     yield return manaRegenChange;
    //     yield return projectileSpeedChange;
    //     yield return projectileLifetimeChange;
    //     yield return knockbackChange;
    //     yield return defense;
    // }

    // IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

[System.Serializable]
public class Heal
{
    public float amount;
    public float timeSpan;

    public Heal(float amount, float timeSpan)
    {
        this.amount = amount;
        this.timeSpan = timeSpan;
    }
}

[System.Serializable]
public class ManaHeal
{
    public float amount;
    public float timeSpan;

    public ManaHeal(float amount, float timeSpan)
    {
        this.amount = amount;
        this.timeSpan = timeSpan;
    }
}