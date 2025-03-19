using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.AI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

/// <summary>
/// Class of info that makes up a room to be generated
/// </summary>
public class RoomInfo
{
    public string name;
    public bool basicShape;
    public int startX;
    public int startZ;
    public List<Vector2Int> coordinatePairs;
}

public class RoomController : MonoBehaviour
{
    public static RoomController instance;

    /// <summary>
    /// Name of current world
    /// </summary>
    [System.NonSerialized]
    public string currentWorldName;

    /// <summary>
    /// Width of one x coordinate (in tiles)
    /// </summary>
    public static int xWidth = 22;
    /// <summary>
    /// Width of one z coordinate
    /// </summary>
    public static int zWidth = 22;

    /// <summary>
    /// Room currently being loaded
    /// </summary>
    RoomInfo currentLoadRoomData;

    /// <summary>
    /// Room player was previously in
    /// </summary>
    Room prevRoom;

    /// <summary>
    /// Room player is currently in
    /// </summary>
    public Room currRoom;

    /// <summary>
    /// Queue of rooms yet to be loaded
    /// </summary>
    Queue<RoomInfo> loadRoomQueue = new Queue<RoomInfo>();
    /// <summary>
    /// Queue of rooms which we can use to unload them
    /// </summary>
    Queue<RoomInfo> allRooms = new Queue<RoomInfo>();

    public Dictionary<Vector2Int, Room> loadedRoomDict = new Dictionary<Vector2Int, Room>();

    bool isLoadingRoom = false;
    
    /// <summary> Has added stuff to generated rooms </summary>
    public bool hasPostProcessed = false;

    void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Initialize the weather system
        
    }

    /// <summary> Calls <see cref="UpdateRoomQueue"/> every frame</summary>
    private void Update() {
        UpdateRoomQueue();
    }

    /// <summary>
    /// Removes doors if done loading rooms or dequeues a RoomInfo from the queue and starts
    /// the <see cref="LoadRoomRoutine"/> with it
    /// </summary>
    void UpdateRoomQueue() 
    {
        if (isLoadingRoom) {
            return;
        }
        // When we're done loading rooms probably, might run more than once so
        // if we want to do something only once, we can set a bool to true
        if (loadRoomQueue.Count == 0) {
            if (!hasPostProcessed) {
                PostProcessRooms(WorldGenerator.instance.endRoomCoords, 0);
                // Start our weather for the world
                // Debug.Log("Setting player height to " + loadedRoomDict[Vector2Int.zero].transform.position.y);
                GameObject.FindGameObjectWithTag("Player").transform.position += loadedRoomDict[Vector2Int.zero].transform.position.y * Vector3.up;
                // Not as easy to do this per room, so we'll do it all at once
                foreach (GameObject i in GameObject.FindGameObjectsWithTag("Environment"))
                {
                    NavMeshSurface surface = i.AddComponent<NavMeshSurface>();
                    // surface.buildHeightMesh = true;
                    surface.agentTypeID = 0;
                    surface.BuildNavMesh();
                }
                CameraController.instance.FinishWorldInitialization();
                
                WeatherController.instance.StartWeather();
                hasPostProcessed = true;
                // TODO: Make a screen show up but then go away here
            }
            return;
        }

        currentLoadRoomData = loadRoomQueue.Dequeue();
        isLoadingRoom = true;
        
        StartCoroutine(LoadRoomRoutine(currentLoadRoomData));
    }

    /// <summary>Process rooms after they've been loaded using BFS recursively</summary>
    /// <param name="roomCoords">Vector (x,z) coordinates of room to process</param>
    /// <param name="depth">How far from the final room this is (BFS depth)</param>
    private void PostProcessRooms(Vector2Int roomCoords, int depth)
    {
        if (!loadedRoomDict.ContainsKey(roomCoords)) return;
        Room room = loadedRoomDict[roomCoords];
        if (!room.postProcessed)
        {
            room.postProcessed = true;
            WorldGenerationData data = WorldGenerator.instance.worldGenerationData;
            if (data.yChangeTowardsEnd != 0) room.gameObject.transform.position -= data.yChangeTowardsEnd * Vector3.up * depth;
            room.SetupDoors();
            StartCoroutine(room.SetupPathfinding());
            room.Decorate(data.randomDecorations);
            PostProcessRooms(roomCoords + Vector2Int.left, depth + 1);
            PostProcessRooms(roomCoords + Vector2Int.up, depth + 1);
            PostProcessRooms(roomCoords + Vector2Int.right, depth + 1);
            PostProcessRooms(roomCoords + Vector2Int.down, depth + 1);
        }
    }

    /// <summary>
    /// Loads a room starting at certain coords or stops if room exists at that location.
    /// Enqueues this new room to be loaded by <see cref="LoadRoomRoutine"/>
    /// </summary>
    /// <param name="name">Name of room</param>
    /// <param name="x">Starting x coord</param>
    /// <param name="z">Starting z coord</param>
    /// <param name="coords">Coordinate pairs that new room will take up</param>
    public void LoadRoom(string name, int x, int z, List<Vector2Int> coords)
    {
        if (loadedRoomDict.ContainsKey(new Vector2Int(x, z))) {
            return;
        }

        RoomInfo newRoomData = new RoomInfo();
        newRoomData.name = name;
        newRoomData.startX = x;
        newRoomData.startZ = z;
        newRoomData.coordinatePairs = coords;

        // Debug.Log("Enqueueing new room data at (" + newRoomData.startX + "," + newRoomData.startZ + ")");

        loadRoomQueue.Enqueue(newRoomData);
        allRooms.Enqueue(newRoomData);
    }
    
    /// <summary>
    /// Asynchronously loads a room scene from its RoomInfo via SceneManager
    /// </summary>
    /// <param name="info">RoomInfo to be loaded by SceneManager</param>
    IEnumerator LoadRoomRoutine (RoomInfo info)
    {
        // Debug.Log("TRYING TO LOAD ROOM ROUTINE at " + info.startX + ", " + info.startZ);
        string roomName = currentWorldName + " - " + info.name;

        AsyncOperation loadRoom = SceneManager.LoadSceneAsync(roomName, LoadSceneMode.Additive);

        while (!loadRoom.isDone) {
            yield return new WaitForSeconds(0.01f);
        }
    }

    /// <summary>
    /// Registers a room or might do something weird it if the location is taken. Try uncommenting that part of this
    /// if dungeon generation is being weird or erroring
    /// </summary>
    /// <param name="room">Room to register</param>
    public void RegisterRoom (Room room)
    {
        // if (FindRoom(currentLoadRoomData.startX, currentLoadRoomData.startY) == null)
        // {
            room.startX = currentLoadRoomData.startX;
            room.startZ = currentLoadRoomData.startZ;

            // Set door coords using name
            room.coordinatePairs = WorldGenerator.instance.GetRoomCoordinatePairs(currentLoadRoomData.name);

            room.name = currentWorldName + "-" + currentLoadRoomData.name + " " + room.startX + ", " + room.startZ;
            room.transform.parent = transform;

            foreach (Vector2Int v in room.coordinatePairs)
            {
                room.finalCoords.Add(new Vector2Int(v.x + room.startX, v.y + room.startZ));
            }
            if (room.isRegular) room.transform.position = new Vector3(room.startX * xWidth, 0, room.startZ * zWidth);
            else room.transform.position = new Vector3(room.startX * xWidth - .5f, 0, room.startZ * zWidth - 1.5f);
            isLoadingRoom = false;

            // if (loadedRooms.Count == 0) {
            //     CameraController.instance.currRoom = room;
            // }

            for (int i = 0; i < room.coordinatePairs.Count; i++)
            {
                Vector2Int coord = room.coordinatePairs[i];
                loadedRoomDict.Add(new Vector2Int(coord.x + room.startX, coord.y + room.startZ), room);
            }
        // } else {
        //     Debug.Log("Room exists, destroying this one");
        //     Destroy(room.gameObject);
        //     isLoadingRoom = false;
        // }
    }

    /// <summary>
    /// Unloads all the currently loaded rooms in the world
    /// </summary>
    public void UnloadAllRooms()
    {
        // Debug.Log("TRYING TO UNLOAD ROOM ROUTINE at " + info.startX + ", " + info.startY);
        // Debug.Log(SceneManager.sceneCount);
        // for (int i = 1; i < SceneManager.sceneCount; i++)
        // {
            // Debug.Log(SceneManager.GetSceneAt(i).name + "exists in the scenes");
            // SceneManager.GetSceneAt(i);
            // SceneManager.UnloadSceneAsync(i);
        // }
        foreach (RoomInfo room in allRooms)
        {
            string roomName = currentWorldName + " - " + room.name;
            // Debug.Log("Trying to unload" + roomName);
            // SceneManager.UnloadSceneAsync(roomName);
            SceneManager.MergeScenes(SceneManager.GetSceneByName(roomName), SceneManager.GetSceneByName("Player Scene"));
        }
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
        
        isLoadingRoom = false;
        allRooms.Clear();
        loadedRoomDict.Clear();
        loadRoomQueue.Clear();
        hasPostProcessed = false;
    }

    /// <summary>
    /// Finds what the coordinate is relative to the start coord of the room it's in, once it's generated
    /// </summary>
    /// <param name="coord">Coordinate to search for</param>
    /// <returns>The coordinate relative to the room it's in</returns>
    public Vector2Int FindRelativeCoord(Vector2Int coord)
    {
        Room r = loadedRoomDict[new Vector2Int(coord.x, coord.y)];
        return new Vector2Int(coord.x - r.startX, coord.y - r.startZ);
    }

    /// <summary>
    /// Switches camera to new room, enables enemies in new room and disables enemies in previous room,
    /// and resets mana to 100
    /// </summary>
    /// <param name="room">New Room entered</param>
    public void OnPlayerEnterRoom(Room room)
    {
        prevRoom = currRoom;
        // Debug.Log(prevRoom.enemiesInRoom == null);
        if (prevRoom != null) {
            foreach (Enemy enemy in prevRoom.enemiesInRoom)
            {
                // enemy.currState = CharacterState.inactive;
            }
        }
        currRoom = room;
        CameraController.instance.SetCurrRoom(currRoom);
        TeamingEnemy finalTeamer = null;
        foreach (Enemy enemy in currRoom.enemiesInRoom)
        {
            if (enemy == null || enemy.gameObject == null) break;
            if (enemy is TeamingEnemy)
            {
                TeamingEnemy e = enemy.gameObject.GetComponent<TeamingEnemy>();
                e.InitializeEnemy();
                e.currState = e.defaultState;
                finalTeamer = e;
            } else {
                if (enemy != null) enemy.currState = CharacterState.wander;
            }
        }
        if (finalTeamer != null) finalTeamer.SetTeamEstablished();

        if (!currRoom.IsCleared())
        {
            StartCoroutine(currRoom.UnclearRoom());
        }
    }
}
