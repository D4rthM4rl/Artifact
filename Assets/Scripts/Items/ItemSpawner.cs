using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{

    [System.Serializable]
    public struct Spawnable
    {
        public GameObject gameObject;
        public float weight;
    }

    public List<Spawnable> items = new List<Spawnable>();
    float totalWeight;

    private MonoBehaviour enemyController;
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
        StartCoroutine(StartHelper());
    }

    /// <summary>
    /// Generates a random item from the weights and spawns after a pause since
    /// we need to wait for its room to be moved to its correct spot and not (0, 0)
    /// </summary>
    private IEnumerator StartHelper()
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

        enemyController = gameObject.GetComponent<Enemy>();
        if (!enemyController) {
            yield return new WaitForSeconds(0.5f);
            SpawnItem();
        }
    }

    /// <summary>
    /// Spawns the randomly chosen item at the item spawner location
    /// </summary>
    public void SpawnItem() {
        GameObject i = Instantiate(items[chosenIndex].gameObject, transform.position, Quaternion.identity, transform.parent) as GameObject;
        i.SetActive(true);
        i.layer = 3; // Item layer
        GameController.numItemsSpawned++;
        WeaponItemController weaponController = i.GetComponent<WeaponItemController>();
        if (weaponController) {weaponController.enabled = true;}
    }
}
