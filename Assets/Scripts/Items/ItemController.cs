using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class ItemController : MonoBehaviour
{    
    public ItemInfo info;
    [HideInInspector]
    public GameObject prefab;
    

    /// <summary>
    /// Sets up the itemController with sprite, color, collider, and light
    /// </summary>
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
        Light light = gameObject.AddComponent<Light>();
        light.color = GetColor();
        light.intensity = 15;
        light.range = 3;
    }

    /// <summary>
    /// Gets the <see cref="Color"/> of the item based on its rarity
    /// </summary>
    /// <returns>The color that corresponds to the rarity</returns>
    protected Color GetColor()
    {
        switch (info.rarity)
        {
            case Rarity.Red:
                return Color.red;
            case Rarity.Orange:
                return new Color(1.0f, 0.5f, 0.0f);
            case Rarity.Yellow:
                return Color.yellow;
            case Rarity.Green:
                return Color.green;
            case Rarity.Blue:
                return Color.blue;
            case Rarity.Indigo:
                return new Color(0.29f, 0.0f, 0.51f);
            case Rarity.Violet:
                return new Color(0.93f, 0.51f, 0.93f);
            case Rarity.Pink:
                return new Color(1.0f, 0.75f, 0.8f);
            case Rarity.White:
                return Color.white;
            case Rarity.Black:
                return Color.black;
        }
        Debug.LogError("Rarity not set for " + gameObject.name);
        return Color.white;
    }

    /// <summary>
    /// When Item is collected, apply stat changes and effects and give Character
    /// the special traits
    /// </summary>
    /// <param name="collision">What collided with the item (might not be a Character</param>
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.GetComponent<Character>())
        {
            Character c = collision.gameObject.GetComponent<Character>();
            c.gameObject.AddComponent<ItemSpawner>().items = new List<ItemSpawner.Spawnable> 
                {new ItemSpawner.Spawnable {itemSpawn = prefab, weight = 1}};
            c.AddStatChange(info.statChanges);
            foreach (Effect e in info.Effects) 
            {
                if (e.level == 0)
                {
                    Debug.LogError("Effect level is 0 for " + info.itemName);
                } else if (e.duration == 0)
                {
                    Debug.LogWarning("Effect duration is 0 for " + info.itemName);
                } else if (e.particles == null)
                {
                    Debug.LogWarning("Effect particles is null for " + info.itemName);
                }
                c.AddAttackEffect(e);
            }
            foreach (SpecialTrait trait in GetComponents<SpecialTrait>())
            {
                SpecialTrait addedTrait = c.gameObject.AddComponent(trait.GetType()) as SpecialTrait;
                // Copy all the fields over and then set the user to the character who got it

                // Get all instance fields (public and non-public)
                FieldInfo[] fields = trait.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (FieldInfo field in fields)
                {
                    // Copy the field value from the original trait to the new trait
                    object value = field.GetValue(trait);
                    field.SetValue(addedTrait, value);
                }

                addedTrait.user = c;
            }

            Destroy(gameObject);
        }
    }
}
