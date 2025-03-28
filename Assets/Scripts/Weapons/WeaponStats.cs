using UnityEngine;

[CreateAssetMenu(fileName = "WeaponStats.asset", menuName = "ItemStats/WeaponStats")]
[System.Serializable]
public class WeaponStats : ScriptableObject
{
    public float damage;
    public float cooldown;
    public float manaUse;
    public float knockback;
    public float size = 1;
    [Tooltip("Whether the stats should only apply while the weapon is equipped")]
    public bool statsOnlyWhileUsing = false;

    [HideInInspector]
    public CharacterStats userStats;
}
