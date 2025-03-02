using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    public float damage;
    public float cooldown;
    public float manaUse = 10f;
    public float knockback;
    public float size;

    public bool inUse = false;
    public bool isSelected = false;
    public Character user;

    /// <summary>Set a weapon to be the one in use</summary>
    public void SetUnselected()
    {
        GetComponent<SpriteRenderer>().enabled = false;
        isSelected = false;
    }

    /// <summary>Set a weapon to be not in use</summary>
    public void SetSelected()
    {
        GetComponent<SpriteRenderer>().enabled = true;
        isSelected = true;
    }
}