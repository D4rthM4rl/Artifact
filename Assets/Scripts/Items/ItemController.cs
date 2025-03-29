using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemController : MonoBehaviour
{    
    public ItemStats itemStats;
    
    public List<GameObject> newTraits = new List<GameObject>();
    public List<Effect> effects = new List<Effect>();
    

    // Start is called before the first frame update
    void Start()
    {
        if (itemStats == null) 
        {
            Debug.LogError("ItemStats is not set for " + gameObject.name);
            return;
        }
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.sprite = itemStats.itemImage;
        if (GetComponent<Renderer>()) GetComponent<Renderer>().material.SetColor("_BaseColor", itemStats.tint);
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
            c.AddStatChange(itemStats.statChanges);
            foreach (Effect e in effects) {c.AddAttackEffect(e);}

            // if (gameObject.GetComponent<SpecialTrait>()) {
            //     c.gameObject.AddComponent(gameObject.GetComponent<SpecialTrait>().GetType());
            // }
            Destroy(gameObject);
        }
    }

    public void ApplyStatChanges(Character c, ItemStats s)
    {
        // SpecialTrait newTrait = item.newTrait;
        // If we're multiplying by 0 it's probably an accident (default value) so we ignore it.
        
    }
}
