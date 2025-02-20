using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{

    [System.Serializable]
    public struct ItemSpawn
    {
        public ItemDetails item;
        public float weight;
    }

    public List<ItemSpawn> items = new List<ItemSpawn>();
    float totalWeight;

    private Enemy enemyController;
    private int chosenIndex;

    void Awake()
    {
        totalWeight = 0;
        foreach (ItemSpawner.ItemSpawn spawnable in items)
        {
            totalWeight += spawnable.weight;
        }
    }

    // Start is called before the first frame update
    void Start() {StartCoroutine(StartHelper());}

    /// <summary>
    /// Generates a random item from the weights and spawns after a pause since
    /// we need to wait for its room to be moved to its correct spot and not (0, 0)
    /// </summary>
    private IEnumerator StartHelper()
    {
        if (items.Count == 0 || totalWeight == 0) yield return null;
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

        enemyController = gameObject.GetComponent<Enemy>();
        if (!enemyController) {
            yield return new WaitForSeconds(0.5f);
            SpawnItem();
        }
    }

    /// <summary>Spawns the randomly chosen item at the item spawner location</summary>
    public void SpawnItem() {
        if (items[chosenIndex].item == null) {
            Debug.LogWarning("ItemSpawner: Item is null");
            return;
        }
        // GameObject i = Instantiate(items[chosenIndex].item.prefab, transform.position, Quaternion.identity);
        GameObject i = new GameObject(items[chosenIndex].item.name, typeof(ItemController));
        i.GetComponent<ItemController>().item = items[chosenIndex].item;
        i.layer = 6; // Item layer
        GameController.numItemsSpawned++;
        WeaponItemController weaponController = i.GetComponent<WeaponItemController>();
        if (weaponController) {weaponController.enabled = true;}
    }
}