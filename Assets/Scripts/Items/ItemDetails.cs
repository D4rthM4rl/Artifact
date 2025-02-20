using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDetails.asset", menuName = "ItemDetails")]
public class ItemDetails : ScriptableObject
{
    public string itemName;
    public string description;
    public int rarity;
    public Sprite itemImage;
    
    public Heal heal = new Heal(0, 0);
    public StatChange maxHealthChange = new StatChange(0, false);
    public StatChange moveSpeedChange = new StatChange(0, false);
    public StatChange attackDamageChange = new StatChange(0, false);
    public StatChange attackRateChange = new StatChange(0, false);
    public StatChange attackSizeChange = new StatChange(0, false);

    public MonoBehaviour newTrait;
    public List<Effect> effects = new List<Effect>();

    public GameObject prefab;
}