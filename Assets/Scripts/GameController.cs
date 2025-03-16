using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController instance;
    
    private bool snowflakeCollected = false;
    private bool infinityCollected = false;
    public static System.Random seededRandom;
    public static System.Random itemRandom;
    [System.NonSerialized]
    public static int numItemsSpawned = 0;

    public GameObject weatherController;

    public Material particleMaterial;
    public Material defaultMaterial;
    public Material litMaterial;

    public GameObject slowGrass;

    public List<string> collectedNames = new List<string>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    /// <summary>
    /// Adds item (ItemController) to collected items
    /// </summary>
    /// <param name="item">Item to be added</param>
    public void UpdateCollectedItems(ItemController item)
    {
        collectedNames.Add(item.item.name);

        foreach(string i in collectedNames)
        {
            switch(i)
            {
                case "Snowflake":
                    snowflakeCollected = true;
                    break;
                case "Infinity":
                    infinityCollected = true;
                    break;
            }
        }
    }
}
