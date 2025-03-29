using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region Single StatChange Class

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

#endregion
#region StatChanges Class

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
}

#endregion
#region Heal Classes

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
#endregion