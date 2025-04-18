using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public static WorldGenerator instance;
    /// <summary>
    /// Data about the world to generate
    /// </summary>
    public WorldGenerationData worldGenerationData;
    /// <summary>
    /// Seed shown to/chosen by user, used to generate <see cref="actualSeed"/>
    /// </summary>
    public int seed;
    /// <summary>
    /// Number we actually use for randoms, not visible to user
    /// </summary>
    public int actualSeed;
    /// <summary>
    /// Whether to generate a new random seed or use inputted one (true is generate new one)
    /// </summary>
    public bool randomSeed;
    private System.Random random;
    /// <summary>
    /// All coordinates visited by the dungeon crawlers
    /// </summary>
    private List<Vector2Int> dungeonRooms;
    /// <summary>
    /// All coordinates used by any room being spawned
    /// </summary>
    private List<Vector2Int> coordinatesTakenSoFar = new List<Vector2Int>();
    public Vector2Int endRoomCoords;

    private void Awake() {
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Upon spawn, it generates a seed and generates the first world
    /// </summary>
    private void Start() 
    {
        if (randomSeed) seed = Random.Range(int.MinValue, int.MaxValue);
        // Debug.Log(randomSeed);
        actualSeed = GetWorldSeed(seed);
        random = new System.Random(actualSeed);
        GameController.seededRandom = random;
        GameController.itemRandom = new System.Random(random.Next());
        GameController.enemyRandom = new System.Random(random.Next());
        GenerateWorld();
    }

    /// <summary>
    /// Creates a blueprint of the world from the world gen data and then spawns the rooms
    /// </summary>
    public void GenerateWorld()
    {
        RoomController.instance.currentWorldName = worldGenerationData.worldName;
        RoomController.xWidth = worldGenerationData.xRoomWidth + (worldGenerationData.wallsSeparateRooms ? 10 : 0);
        RoomController.zWidth = worldGenerationData.zRoomWidth + (worldGenerationData.wallsSeparateRooms ? 10 : 0);
        dungeonRooms = DungeonCrawlerController.GenerateDungeon(worldGenerationData, random, actualSeed);
        SpawnRooms(dungeonRooms);
    }

    /// <summary>
    /// Spawns the given rooms for the world
    /// </summary>
    /// <param name="rooms">What rooms to spawn and where</param>
    private void SpawnRooms (IEnumerable<Vector2Int> rooms)
    {
        RoomController.instance.LoadRoom("Start", 0, 0, new List<Vector2Int>(){Vector2Int.zero});
        coordinatesTakenSoFar.Add(Vector2Int.zero);
        int farX = 0;
        int farZ = 0;
        float maxDist = 0;
        foreach (Vector2Int roomLocation in rooms) // Look through room generation coords
        {
            int startX = roomLocation.x;
            int startZ = roomLocation.y;

            string name = GenerateRandomRoom();
            
            List<Vector2Int> coords = GetRoomCoordinatePairs(name);
            bool canGenerateRoom = true;
            int xStartOffset = 0;
            int zStartOffset = 0;
            foreach (Vector2Int startPair in coords)
            {
            // Debug.Log("trying to generate room at (" + (startX - startPair.x) + ", " + (startZ - startPair.y) + ")");
                foreach (Vector2Int relativity in coords)
                {

                    if (coordinatesTakenSoFar.Contains(new Vector2Int(startX - startPair.x + relativity.x, startZ - startPair.y + relativity.y)))
                    {
                        // Debug.Log("    Not generating " + name + " room because it takes up (" + (startX - startPair.x + relativity.x) + ", " + (startY - startPair.y + relativity.y));
                        canGenerateRoom = false;
                        break;
                    } else {
                        canGenerateRoom = true;

                        // Debug.Log("Yea generating " + name + " room at (" + (startX + startPair.x + relativity.x) + ", " + (startY + startPair.y + relativity.y));
                    }
                }
                if (canGenerateRoom) 
                {
                    // Debug.Log("    Yea generating " + name + " room at (" + (startX - startPair.x) + ", " + (startY - startPair.y));
                    xStartOffset = startPair.x;
                    zStartOffset = startPair.y;
                    break;
                }
            }
            if (!canGenerateRoom && !coordinatesTakenSoFar.Contains(new Vector2Int(startX, startZ))) {
                canGenerateRoom = true; // If it can't gen big room but can do 1x1 room
                name = "Empty";
                coords = GetRoomCoordinatePairs(name);
            }
            // Debug.Log("Finally generating " + name + " room at (" + startX + ", " + startY + ")?");
            if (canGenerateRoom)
            {
                foreach (Vector2Int coordPair in coords)
                {
                    int x = startX - xStartOffset + coordPair.x;
                    int z = startZ - zStartOffset + coordPair.y;
                    coordinatesTakenSoFar.Add(new Vector2Int(x, z));
                    float distFromOrigin = Mathf.Sqrt(x * x + z * z);
                    if (distFromOrigin > maxDist) {
                        farX = x;
                        farZ = z;
                        maxDist = distFromOrigin;
                    }
                }
                RoomController.instance.LoadRoom(name, startX - xStartOffset, startZ - zStartOffset, coords);
            }
        }

        endRoomCoords = GenerateEndCoords(farX, farZ);
        coordinatesTakenSoFar.Add(endRoomCoords);
        RoomController.instance.LoadRoom("End", endRoomCoords.x, endRoomCoords.y, new List<Vector2Int>(){Vector2Int.zero});
        dungeonRooms.Clear();
        coordinatesTakenSoFar.Clear();
    }
    
    /// <summary>
    /// Using weights of each room type from the world, picks a random one
    /// </summary>
    /// <returns>Random room type name from the world</returns>
    private string GenerateRandomRoom()
    {
        int totalWeight = 0;
        List<RoomType> roomTypes = worldGenerationData.roomTypes;
        foreach (RoomType r in roomTypes)
        {
            totalWeight += r.weight;
        }
        int randomNumber = random.Next(0, totalWeight);
        for (int i = 0; i < roomTypes.Count; i++)
        {
            if (randomNumber < roomTypes[i].weight)
            {
                return roomTypes[i].type;
            }
            randomNumber -= roomTypes[i].weight;
        }
        Debug.LogError("Bad room weight");
        return null;
    }

    /// <summary>
    /// Picks the coordinate to place the final room, as far away as possible
    /// </summary>
    /// <param name="farX">Farthest away a room is in x</param>
    /// <param name="farZ">Farthest away a room is in y</param>
    /// <returns>Coordinate pair for final (farthest) room</returns>
    private Vector2Int GenerateEndCoords(int farX, int farZ)
    {
        int xDisplace = 0;
        int zDisplace = 0;
        if (Mathf.Abs(farX) >= Mathf.Abs(farZ))
        {
            if (farX > 0) {xDisplace = 1;}
            else if (farX < 0) {xDisplace = -1;}
            else {xDisplace = 5 - (2 * random.Next(2, 4));} // Random -1 or 1
        } else
        {
            if (farZ > 0) {zDisplace = 1;}
            else if (farZ < 0) {zDisplace = -1;}
            else {zDisplace = 5 - (2 * random.Next(2, 4));} // Random -1 or 1
        }
        int xEnd = farX + xDisplace;
        int zEnd = farZ + zDisplace;
        return new Vector2Int(xEnd, zEnd);
    }

    /// <summary>
    /// Gets a seed to be used for the current world generation
    /// </summary>
    /// <param name="s">The overall seed for generation</param>
    /// <returns>A new int that used the original seed and current world name from dungeonGenerationData</returns>
    private int GetWorldSeed(int s)
    {
        string world = worldGenerationData.worldName;
        int worldHash = 31;
        int index = 0;
        int n = world.Length;
        // Make a hashcode for the world name
        foreach (char c in world)
        {
            worldHash += c * (int)Mathf.Pow(31, (n - index + 1));
            index++;
        }
        int newSeed = s + worldHash;

        // Debug.Log("Seed: " + s + " + world hash: " + worldHash + " = " + (s + worldHash));
        return newSeed;
    }

    /// <summary>
    /// Returns coordinate pairs the room takes up relative to a hard-coded (0,0) origin of the room
    /// </summary>
    /// <param name="name">Name of room to get coordinate pairs of</param>
    /// <returns>List of Vector2Int coordinates</returns>
    public List<Vector2Int> GetRoomCoordinatePairs(string name) 
    {
        foreach (RoomType t in worldGenerationData.roomTypes)
        {
            if (t.type.Equals(name))
            return t.coordinates;
        }
        Debug.LogError("No room type labelled " + name + " found");
        return null;
    }
}