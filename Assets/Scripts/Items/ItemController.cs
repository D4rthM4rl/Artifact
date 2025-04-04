using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemController : MonoBehaviour
{    
    public ItemInfo info;
    [HideInInspector]
    public GameObject prefab;
    

    // Start is called before the first frame update
    void Start()
    {
        if (info == null) 
        {
            Debug.LogError("ItemInfo is not set for " + gameObject.name);
            return;
        }
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.sprite = info.itemImage;
        if (GetComponent<Renderer>()) GetComponent<Renderer>().material.SetColor("_BaseColor", info.tint);
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
            c.gameObject.AddComponent<ItemSpawner>().items = new List<ItemSpawner.Spawnable> 
                {new ItemSpawner.Spawnable {itemSpawn = prefab, weight = 1}};
            c.AddStatChange(info.statChanges);
            foreach (Effect e in info.Effects) {c.AddAttackEffect(e);}

            // if (gameObject.GetComponent<SpecialTrait>()) {
            //     c.gameObject.AddComponent(gameObject.GetComponent<SpecialTrait>().GetType());
            // }
            Destroy(gameObject);
        }
    }

    public void ApplyStatChanges(Character c, ItemInfo s)
    {
        // SpecialTrait newTrait = item.newTrait;
        // If we're multiplying by 0 it's probably an accident (default value) so we ignore it.
        
    }
}
