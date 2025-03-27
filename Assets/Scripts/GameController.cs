using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController instance;
    
    public static System.Random seededRandom;
    public static System.Random itemRandom;
    public static System.Random enemyRandom;
    [System.NonSerialized]
    public static int numItemsSpawned = 0;
    [System.NonSerialized]
    public static int numEnemiesSpawned = 0;

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

    public static void ApplyGravity(Rigidbody rb)
    {
        rb.AddForce(Vector3.down * 9.81f, ForceMode.Acceleration);
    }
}
