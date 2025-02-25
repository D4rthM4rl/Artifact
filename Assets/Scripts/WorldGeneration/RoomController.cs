using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Class of info that makes up a room to be generated
/// </summary>
public class RoomInfo
{
    public string name;
    public bool basicShape;
    public int startX;
    public int startY;
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
    public static int xWidth = 29;
    /// <summary>
    /// Width of one y coordinate
    /// </summary>
    public static int yWidth = 19;

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
    Room currRoom;

    /// <summary>
    /// Queue of rooms yet to be loaded
    /// </summary>
    Queue<RoomInfo> loadRoomQueue = new Queue<RoomInfo>();
    /// <summary>
    /// Queue of rooms which we can use to unload them
    /// </summary>
    Queue<RoomInfo> allRooms = new Queue<RoomInfo>();
    /// <summary>
    /// List of rooms which are already loaded
    /// </summary>
    public List<Room> loadedRooms = new List<Room>();

    bool isLoadingRoom = false;
    /// <summary>
    /// Has removed all unnecessary doors 
    /// </summary>
    bool doorsRemoved = false;

    void Start()
    {
        // LoadRoom("Start", 0, 0);
        // LoadRoom("Empty", 1, 0);
    }

    /// <summary>
    /// Calls <see cref="UpdateRoomQueue"/> every frame
    /// </summary>
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
        if (loadRoomQueue.Count == 0) {
            if (!doorsRemoved) { // This method of removing doors waits til the end so it's kinda late/slow 
                doorsRemoved = true;
                foreach (Room room in loadedRooms)
                {
                    room.SetupDoors();
                }
            }
            return;
        }

        currentLoadRoomData = loadRoomQueue.Dequeue();
        isLoadingRoom = true;
        
        StartCoroutine(LoadRoomRoutine(currentLoadRoomData));
    }

    void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Loads a room starting at certain coords or stops if room exists at that location.
    /// Enqueues this new room to be loaded by <see cref="LoadRoomRoutine"/>
    /// </summary>
    /// <param name="name">Name of room</param>
    /// <param name="x">Starting x coord</param>
    /// <param name="y">Starting y coord</param>
    /// <param name="coords">Coordinate pairs that new room will take up</param>
    public void LoadRoom (string name, int x, int y, List<Vector2Int> coords)
    {
        if (FindRoom(x, y) != null) {
            return;
        }

        RoomInfo newRoomData = new RoomInfo();
        newRoomData.name = name;
        newRoomData.startX = x;
        newRoomData.startY = y;
        newRoomData.coordinatePairs = coords;

        // Debug.Log("Enqueueing new room data at (" + newRoomData.startX + "," + newRoomData.startY + ")");

        loadRoomQueue.Enqueue(newRoomData);
        allRooms.Enqueue(newRoomData);
    }
    
    /// <summary>
    /// Asynchronously loads a room scene from its RoomInfo via SceneManager
    /// </summary>
    /// <param name="info">RoomInfo to be loaded by SceneManager</param>
    /// <returns></returns>
    IEnumerator LoadRoomRoutine (RoomInfo info)
    {
        // Debug.Log("TRYING TO LOAD ROOM ROUTINE at " + info.startX + ", " + info.startY);
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
            room.startY = currentLoadRoomData.startY;

            // Set door coords using name
            room.coordinatePairs = WorldGenerator.instance.GetRoomCoordinatePairs(currentLoadRoomData.name);

            room.name = currentWorldName + "-" + currentLoadRoomData.name + " " + room.startX + ", " + room.startY;
            room.transform.parent = transform;

            foreach (Vector2Int v in room.coordinatePairs)
            {
                room.finalCoords.Add(new Vector2Int(v.x + room.startX, v.y + room.startY));
            }
            if (room.isRegular) room.transform.position = new Vector3(room.startX * xWidth, room.startY * yWidth, 0);
            else room.transform.position = new Vector3(room.startX * xWidth - .5f, room.startY * yWidth - 1.5f, 0);
            isLoadingRoom = false;

            if (loadedRooms.Count == 0) {
                CameraController.instance.currRoom = room;
            }

            loadedRooms.Add(room);
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
        for (int i = 1; i < SceneManager.sceneCount; i++)
        {
            // Debug.Log(SceneManager.GetSceneAt(i).name + "exists in the scenes");
            // SceneManager.GetSceneAt(i);
            // SceneManager.UnloadSceneAsync(i);
        }
        foreach (RoomInfo room in allRooms)
        {
            string roomName = currentWorldName + " - " + room.name;
            Debug.Log("Trying to unload" + roomName);
            // SceneManager.UnloadSceneAsync(roomName);
            SceneManager.MergeScenes(SceneManager.GetSceneByName(roomName), SceneManager.GetSceneByName("Player Scene"));
        }
        for(int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
        isLoadingRoom = false;
        allRooms.Clear();
        loadedRooms.Clear();
        loadRoomQueue.Clear();
        doorsRemoved = false;
    }

    /// <summary>
    /// Returns room at given coordinates or null if nothing found there
    /// </summary>
    /// <param name="x">X coord to check</param>
    /// <param name="y">Y coord to check</param>
    /// <returns> Returns null if room not found</returns>
    public Room FindRoom (int x, int y)
    {
        // Debug.Log("Finding (" + x + ", " + y + ")");
        foreach (Room r in loadedRooms)
        {
            foreach (Vector2Int v in r.finalCoords)
            {
                // Debug.Log(v.x + " " + v.y);
                if (v.x == x && v.y == y) {
                    // Debug.Log("Found");
                    return r;
                }
            }
        }
        // Debug.Log("Not found");
        return null;
    }

    /// <summary>
    /// Finds what the coordinate is relative to the start coord of the room it's in, once it's generated
    /// </summary>
    /// <param name="coord">Coordinate to search for</param>
    /// <returns>The coordinate relative to the room it's in</returns>
    public Vector2Int FindRelativeCoord(Vector2Int coord)
    {
        Room r = FindRoom(coord.x, coord.y);
        return new Vector2Int(coord.x - r.startX, coord.y - r.startY);
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
                enemy.currState = CharacterState.inactive;
            }
        }
        CameraController.instance.currRoom = room;
        currRoom = room;
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
