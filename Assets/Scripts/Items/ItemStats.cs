using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemStats.asset", menuName = "ItemStats/ItemStats")]
[System.Serializable]
public class ItemStats : ScriptableObject
{
    public string itemName;
    public string description;
    public int rarity;
    public Sprite itemImage;
    public Color tint = Color.white;
    
    [SerializeField]
    StatChanges statChanges = new StatChanges();
}