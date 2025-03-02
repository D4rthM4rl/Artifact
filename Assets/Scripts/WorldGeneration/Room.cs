using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Room : MonoBehaviour
{
    public int startX;
    public int startZ;
    public List<Vector2Int> coordinatePairs = new List<Vector2Int>(); // Coords relative to (0, 0)
    public int xWidth;
    public int zWidth;
    // public int height;
    private bool cleared = false;

    /// <summary>
    /// The enemies that spawn into/start in the room
    /// </summary>
    public List<Enemy> enemiesInRoom = new List<Enemy>();

    /// <summary>
    /// Coordinate pairs room ends up taking up when generated
    /// </summary>
    public List<Vector2Int> finalCoords = new List<Vector2Int>();

    /// <summary>
    /// True if it's just a basic 1x1 coordinate rectangle
    /// </summary>
    public bool isRegular = true;

    public Dictionary<Vector2Int, List<Door>> doors = new Dictionary<Vector2Int, List<Door>>();

    /// <summary>
    /// Add all doors and enemies to room to remove and make inactive/active respectively
    /// </summary>
    void Start()
    {
        if (RoomController.instance == null)
        {
            Debug.LogError("You pressed play in the wrong scene!");
            return;
        }

        Door[] ds = GetComponentsInChildren<Door>();
        foreach (Door d in ds)
        {
            Vector2Int doorOffset = new Vector2Int(d.xOffset, d.zOffset);
            if (!doors.ContainsKey(doorOffset))
            {
                doors.Add(doorOffset, new List<Door>());
            } 
            doors[doorOffset].Add(d);
        }

        foreach (Enemy e in GetComponentsInChildren<Enemy>()) {
            e.currState = CharacterState.inactive;
            enemiesInRoom.Add(e);
        }

        RoomController.instance.RegisterRoom(this);
    }

    /// <summary>
    /// Remove all doors in room which don't have room on the other side of it
    /// </summary>
    public void SetupDoors()
    {
        foreach (Vector2Int coord in doors.Keys)
        {
            List<Door> coordDoors = doors[coord];
            // TODO: Change door type based on type of room

            foreach (Door d in coordDoors)
            {
                Room r;
                Door.DoorDirection o;
                switch (d.doorDirection)
                {
                    case Door.DoorDirection.right:
                        r = d.GetRight(startX, startZ);
                        o = Door.DoorDirection.left;
                        SetupDoor(r, d, o, Vector2Int.right);
                        break;
                    case Door.DoorDirection.left:
                        r = d.GetLeft(startX, startZ);
                        o = Door.DoorDirection.right;
                        SetupDoor(r, d, o, Vector2Int.left);
                        break;
                    case Door.DoorDirection.top:
                        r = d.GetTop(startX, startZ);
                        o = Door.DoorDirection.bottom;
                        SetupDoor(r, d, o, Vector2Int.up);
                        break;
                    case Door.DoorDirection.bottom:
                        r = d.GetBottom(startX, startZ);
                        o = Door.DoorDirection.top;
                        SetupDoor(r, d, o, Vector2Int.down);
                        break;
                }
            }
        }
    }
    
    /// <summary>
    /// Helper for SetupDoors to change a door to what it should be based on its room
    /// </summary>
    /// <param name="r">Room on other side of door</param>
    /// <param name="d">Door to adjust</param>
    /// <param name="otherRoomOffset">Difference from door's room to other door's room</param>
    private void SetupDoor(Room r, Door d, Door.DoorDirection o, Vector2Int otherRoomOffset)
    {
        if (r == null) d.SetWall();
        else
        {
            if (!d.isLocked) d.SetOpen();
            // The coordinate of the door to be adjusted
            Vector2Int doorFinalCoord = new Vector2Int(startX + d.xOffset, startZ + d.zOffset);
            // Debug.Log("Coord of door to connect: " + doorFinalCoord);
            // The coordinate offset from the other room of the door to be connected to
            Vector2Int otherDoorOffset = doorFinalCoord + otherRoomOffset - new Vector2Int(r.startX, r.startZ);
            // Debug.Log("COord of other door to connect to: " + otherDoorOffset);

            foreach (Door otherDoor in r.doors[otherDoorOffset])
            {
                if ((Door.DoorDirection)otherDoor.doorDirection == o)
                {
                    d.SetConnection(otherDoor);
                    return;
                }
            }
            Debug.LogError("Couldn't set up door correctly");
        } 
    }

    /// <summary>
    /// Draws a red box outlining where the rooms is
    /// </summary>
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 roomCenter = GetRoomCenter();
        if (isRegular) Gizmos.DrawWireCube(roomCenter, new Vector3(xWidth, 100, zWidth));
        else Gizmos.DrawWireCube(new Vector3(roomCenter.x + RoomController.xWidth/2, 0, roomCenter.z + RoomController.zWidth), new Vector3(xWidth, 10, zWidth));
    }

    /// <summary>
    /// Gets the center of a room from coordinates
    /// </summary>
    /// <returns></returns>
    public Vector3 GetRoomCenter()
    {
        int numCoords = 0;
        float centerX = 0;
        float centerZ = 0;
        foreach (Vector2Int coord in finalCoords)
        {
            numCoords++;
            centerX += coord.x;
            centerZ += coord.y;
        }
        centerX /= numCoords;
        centerZ /= numCoords;
        return new Vector3(startX * RoomController.xWidth, 0, startZ * RoomController.zWidth);
    }

    /// <summary>
    /// Returns whether room has been cleared
    /// </summary>
    /// <returns>True if room is cleared</returns>
    public bool IsCleared()
    {
        return cleared;
    }

    /// <summary>
    /// Clears a room opening all not locked doors such as when all enemies killed
    /// </summary>
    public void ClearRoom()
    {
        foreach (Vector2Int loc in doors.Keys)
        {
            foreach (Door d in doors[loc])
            {
                d.SetOpen();
            }
        }
    }

    /// <summary>
    /// Closes doors to unclear room until all enemies are dead, then clears
    /// </summary>
    public IEnumerator UnclearRoom()
    {
        foreach (Vector2Int loc in doors.Keys)
        {
            foreach (Door d in doors[loc])
            {
                d.SetClosed();
            }
        }
        // Debug.Log("Unclearing room");
        bool allEnemiesDead = false;
        while (!allEnemiesDead)
        {
            allEnemiesDead = true;
            foreach (Enemy e in enemiesInRoom)
            {
                if (!e.dead)
                {
                    allEnemiesDead = false;
                    break;
                }
            }
            yield return new WaitForSeconds(0.1f);
        }
        ClearRoom();
    }
    
    /// <summary>
    /// Room is entered
    /// </summary>
    /// <param name="other">What entered the room</param>
    void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.tag == "Player") {
            RoomController.instance.OnPlayerEnterRoom(this);
            other.gameObject.GetComponent<Player>().mana = 100;
        }
    }
}
