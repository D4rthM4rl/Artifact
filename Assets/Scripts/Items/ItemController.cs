using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Item
{
    public string name;
    public string description;
    public int rarity;
    public Sprite itemImage;
    public Color tint = Color.white;
}

[System.Serializable]
public class StatChange
{
    public static StatChange same { get;} = new StatChange(0, false);
    public float amount;
    public bool multiplier;

    public StatChange(float amount, bool multiplier)
    {
        this.amount = amount;
        this.multiplier = multiplier;
    }
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

public class ItemController : MonoBehaviour
{    
    public Item item;
    public Heal heal = new Heal(0, 0);
    public StatChange maxHealthChange = new StatChange(0, false);
    public StatChange moveSpeedChange = new StatChange(0, false);
    public StatChange attackDamageChange = new StatChange(0, false);
    public StatChange attackRateChange = new StatChange(0, false);
    public StatChange attackSizeChange = new StatChange(0, false);

    public bool special = false;
    public MonoBehaviour newTrait;
    
    public List<Effect> effects = new List<Effect>();
    

    // Start is called before the first frame update
    void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.sprite = item.itemImage;
        if (GetComponent<Renderer>()) GetComponent<Renderer>().material.SetColor("_BaseColor", item.tint);
        Destroy(GetComponent<Collider>());
        SphereCollider collider = gameObject.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        gameObject.AddComponent<Light>();
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.GetComponent<Character>()) 
        {
            Character c = collision.gameObject.GetComponent<Character>();
            // If we're multiplying by 0 it's probably an accident (default value) so we ignore it.
            if (maxHealthChange.amount != 0 || !maxHealthChange.multiplier) {
                c.ChangeMaxHealth(maxHealthChange.amount, maxHealthChange.multiplier);
            }

            c.Heal(heal.amount, heal.timeSpan);

            if (moveSpeedChange.amount != 0 || !moveSpeedChange.multiplier) {
                c.ChangeMoveSpeed(moveSpeedChange.amount, moveSpeedChange.multiplier);
            }
            if (attackDamageChange.amount != 0 || !attackDamageChange.multiplier) {
                c.ChangeAttackDamage(attackDamageChange.amount, attackDamageChange.multiplier);
            }
            if (attackRateChange.amount != 0 || !attackRateChange.multiplier) {
                c.ChangeAttackRate(attackRateChange.amount, attackRateChange.multiplier);
            }
            if (attackSizeChange.amount != 0 || !attackSizeChange.multiplier) {
                c.ChangeAttackSize(attackSizeChange.amount, attackSizeChange.multiplier);
            }
            foreach (Effect e in effects) {c.AddAttackEffect(e);}

            // if (newTrait != null) {c.gameObject.AddComponent(newTrait.GetType());}
            if (gameObject.GetComponent<SpecialTrait>()) {c.gameObject.AddComponent(gameObject.GetComponent<SpecialTrait>().GetType());}
            Destroy(gameObject);
        }
    }
}
