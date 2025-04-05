using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{

    [System.Serializable]
    public struct Spawnable
    {
        public GameObject itemSpawn;
        public float weight;
        // [HideInInspector]
        // public List<SpecialTrait> specialTraits;
    }

    public List<Spawnable> items = new List<Spawnable>();
    [Tooltip("Chance of spawning the item")]
    [Range(0, 1)]
    /// <summary>
    /// Chance of spawning an item
    /// </summary>
    public float spawnChance = 1;
    private float totalWeight;

    private Character character;
    private int chosenIndex;

    /// <summary>
    /// Gets the total weight of the items it could spawn
    /// </summary>
    void Awake()
    {
        totalWeight = 0;
        foreach (var spawnable in items)
        {
            totalWeight += spawnable.weight;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(TryToSpawn());
    }

    /// <summary>
    /// Generates a random item from the weights and spawns after a pause since
    /// we need to wait for its room to be moved to its correct spot and not (0, 0)
    /// </summary>
    private IEnumerator TryToSpawn()
    {
        // Debug.Log(GameController.seed + GameController.numItemsSpawned);
        while (GameController.itemRandom == null) yield return new WaitForSeconds(0.1f);
        
        // Item generated will keep being same number unless we add this number to get a new random seed
        float pick = (float)GameController.itemRandom.NextDouble() * totalWeight;
        chosenIndex = 0;
        float cumulativeWeight = items[0].weight;

        while(pick > cumulativeWeight && chosenIndex < items.Count - 1)
        {
            chosenIndex++;
            cumulativeWeight += items[chosenIndex].weight;
        }

        // If we have a Character component on this GameObject, that means we
        // need to wait for it to die before spawning the item
        character = gameObject.GetComponent<Character>();
        if (!character) {
            yield return new WaitForSeconds(0.5f);
            SpawnItem();
        }
    }

    /// <summary>
    /// Spawns the randomly chosen item at the item spawner location
    /// </summary>
    public void SpawnItem() {
        Debug.Assert(items[chosenIndex].itemSpawn != null, "Item spawner has a null item for " + gameObject.name);
        GameObject i = Instantiate(items[chosenIndex].itemSpawn, transform.position, Quaternion.identity, transform.parent) as GameObject;
        
        ItemController item = i.GetComponent<ItemController>();
        Debug.Assert(item != null, "ItemController is null");
        item.prefab = items[chosenIndex].itemSpawn;
        i.SetActive(true);
        i.layer = 3; // Item layer
        GameController.numItemsSpawned++;
        WeaponItemController weaponController = i.GetComponent<WeaponItemController>();
        if (weaponController) {weaponController.enabled = true;}
    }
}
