using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    public ItemInfo itemInfo;
    public WeaponStats stats;
    public bool inUse = false;
    public bool isSelected = false;
    public Character user;

    /// <summary>Set a weapon to be the one in use</summary>
    public void SetUnselected()
    {
        Debug.Assert(user != null, "User is null");
        Debug.Assert(itemInfo != null, "ItemStats is null");

        GetComponent<SpriteRenderer>().enabled = false;
        isSelected = false;
        if (stats.statsOnlyWhileUsing) user.RemoveStatChange(itemInfo.statChanges);
    }

    /// <summary>Set a weapon to be not in use</summary>
    public void SetSelected()
    {
        Debug.Assert(user != null, "User is null");
        Debug.Assert(itemInfo != null, "ItemStats is null");
        
        GetComponent<SpriteRenderer>().enabled = true;
        isSelected = true;
        if (stats.statsOnlyWhileUsing) user.AddStatChange(itemInfo.statChanges);
    }
}