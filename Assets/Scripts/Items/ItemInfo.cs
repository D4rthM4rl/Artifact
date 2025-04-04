using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemStats.asset", menuName = "ItemStats/ItemStats")]
[System.Serializable]
public class ItemInfo : ScriptableObject
{
    public string itemName;
    public string description;
    public int rarity;
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