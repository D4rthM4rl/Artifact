using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemStats.asset", menuName = "ItemStats/ItemStats")]
[System.Serializable]
public class ItemInfo : ScriptableObject
{
    public string itemName;
    [SerializeField]
    [TextArea(3, 10)]
    private string description;
    public string Description {get { return description; }}
    /// <summary>Color Rarity from 1 (red) to 10 (black), higher rarity is better item</summary>
    [Tooltip("Color Rarity from 1 (red) to 10 (black), higher rarity is better item")]
    public Rarity rarity;
    public Sprite itemImage;
    public Color tint = Color.white;
    [SerializeField]
    private List<Effect> effects = new List<Effect>();
    public List<Effect> Effects
    {
        get { return effects; }
    }
    
    [SerializeField]
    public StatChanges statChanges = new StatChanges();
}

[System.Serializable]
public enum Rarity
{
    Red,
    Orange,
    Yellow,
    Green,
    Blue,
    Indigo,
    Violet,
    Pink,
    White,
    Black,
}