using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    /// <summary>
    /// An instance of enemy-- the prefab, how many, how big one is, and how common
    /// </summary>
    [System.Serializable]
    public struct EnemyInstance
    {
        /// <summary>What the decoration actually is</summary>
        [Tooltip("Prefab for the decoration")]
        public GameObject gameObject;

        /// <summary>How big the enemy is in each direction</summary>
        [Tooltip("How big an enemy is in each direction")]
        public Vector3 size;

        /// <summary>How many of this enemy minimum can be in a clump</summary>
        [Tooltip("How many of this decoration minimum can be in a clump")]
        public int packSizeMin;
        /// <summary>How many of this decoration maximum can be in a clump</summary>
        [Tooltip("How many of this enemy maximum can be in a group")]
        public int packSizeMax;

        public int weight;
    }

    public List<EnemyInstance> enemies = new List<EnemyInstance>();
    float totalWeight;

    public Room room;

    private Enemy enemyController;

    private int chosenIndex;

    /// <summary>
    /// Gets the total weight of the items it could spawn
    /// </summary>
    void Awake()
    {
        totalWeight = 0;
        foreach (EnemyInstance spawnable in enemies)
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
        while (GameController.enemyRandom == null || room == null) yield return new WaitForSeconds(0.1f);
        
        // Item generated will keep being same number unless we add this number to get a new random seed
        float pick = (float)GameController.enemyRandom.NextDouble() * totalWeight;
        chosenIndex = 0;
        float cumulativeWeight = enemies[0].weight;

        while(pick > cumulativeWeight && chosenIndex < enemies.Count - 1)
        {
            chosenIndex++;
            cumulativeWeight += enemies[chosenIndex].weight;
        }

        enemyController = gameObject.GetComponent<Enemy>();
        if (!enemyController) {
            yield return new WaitForSeconds(0.5f);
            SpawnEnemy();
        }
    }

    /// <summary>
    /// Spawns the randomly chosen item at the item spawner location
    /// </summary>
    public void SpawnEnemy() {
        Dictionary<Vector3, GameObject> positionsTaken = new Dictionary<Vector3, GameObject>();
        EnemyInstance e = enemies[chosenIndex];

        Vector3 pos;
        bool posTaken = false;
        List<Vector3> positionsMaybeTaken = new List<Vector3>();
        for (int i = 0; i < Random.Range(e.packSizeMin, e.packSizeMax + 1); i++)
        {
            positionsMaybeTaken.Clear();
            pos = new Vector3(Random.Range((-room.xWidth+e.size.x)/2, (room.xWidth-e.size.x)/2), 0,
                Random.Range((-room.zWidth+e.size.z)/2, (room.zWidth-e.size.z)/2));
            if (positionsTaken.ContainsKey(pos + e.size)) continue;
            for (float x = e.size.x; x > 0; x -= .5f)
            {
                for (float z = e.size.z; z > 0; z -= .5f)
                {
                    if (positionsTaken.ContainsKey(pos + new Vector3(x, 0, z))) 
                    {
                        posTaken = true;
                        break;
                    }
                    positionsMaybeTaken.Add(pos + new Vector3(x, 0, z));
                }
                if (posTaken) break;
            }
            if (posTaken) continue;
            // If we checked all necessary positions and this placement is valid
            foreach (Vector3 position in positionsMaybeTaken)
            {
                positionsTaken.Add(position, e.gameObject);
            }
            GameController.numEnemiesSpawned++;
            GameObject enemyObj = Instantiate(e.gameObject, pos + Vector3.up + transform.position, Quaternion.identity, transform);
            Enemy enemy = enemyObj.GetComponent<Enemy>();
            Debug.Assert(enemy != null, "Enemy is null");
            enemy.prefab = enemyObj;
            room.enemiesInRoom.Add(enemy);
            room.SetupPathfinding(enemy);
            enemy.currState = CharacterState.inactive;
        }
    }
}
