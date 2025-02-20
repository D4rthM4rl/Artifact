using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDetails.asset", menuName = "ItemDetails/Weapon")]
public class WeaponDetails : ItemDetails
{
    // Make weapons be subtypes of items
    public float damage;
    public float cooldown;
    public float manaUse = 10f;
    public float knockback;
    public float size;

    public bool inUse = false;
    public bool isSelected = false;
    public Character user;
}