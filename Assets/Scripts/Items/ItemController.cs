using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public ItemDetails item;
    public List<SpecialTrait> newTrait = new List<SpecialTrait>();

    // Start is called before the first frame update
    void Start()
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer == null) {renderer = gameObject.AddComponent<SpriteRenderer>();}
        renderer.sprite = item.itemImage;
        Destroy(GetComponent<PolygonCollider2D>());
        PolygonCollider2D collider = gameObject.AddComponent<PolygonCollider2D>();
        collider.isTrigger = true;
        foreach (SpecialTrait t in GetComponents<SpecialTrait>()) {newTrait.Add(t);}
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<Character>()) 
        {
            Character c = collision.gameObject.GetComponent<Character>();
            // Make whatever picked up the item drop it on death
            c.gameObject.AddComponent<ItemSpawner>().items = new List<ItemSpawner.ItemSpawn> 
                {new ItemSpawner.ItemSpawn {item = item, weight = 1}};
            // If we're multiplying by 0 it's probably an accident (default value) so we ignore it.
            if (item.maxHealthChange.amount != 0 || !item.maxHealthChange.multiplier) {
                c.ChangeMaxHealth(item.maxHealthChange.amount, item.maxHealthChange.multiplier);
            }

            c.Heal(item.heal.amount, item.heal.timeSpan);

            if (item.moveSpeedChange.amount != 0 || !item.moveSpeedChange.multiplier) {
                c.ChangeMoveSpeed(item.moveSpeedChange.amount, item.moveSpeedChange.multiplier);
            }
            if (item.attackDamageChange.amount != 0 || !item.attackDamageChange.multiplier) {
                c.ChangeAttackDamage(item.attackDamageChange.amount, item.attackDamageChange.multiplier);
            }
            if (item.attackRateChange.amount != 0 || !item.attackRateChange.multiplier) {
                c.ChangeAttackRate(item.attackRateChange.amount, item.attackRateChange.multiplier);
            }
            if (item.attackSizeChange.amount != 0 || !item.attackSizeChange.multiplier) {
                c.ChangeAttackSize(item.attackSizeChange.amount, item.attackSizeChange.multiplier);
            }
            foreach (Effect e in item.effects) {c.AddAttackEffect(e);}

            foreach (SpecialTrait t in newTrait) {c.gameObject.AddComponent(t.GetType());}
            Destroy(gameObject);
        }
    }
}