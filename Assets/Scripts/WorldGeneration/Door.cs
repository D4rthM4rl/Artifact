using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Door : MonoBehaviour
{
    public int xOffset;
    public int zOffset;
    public Vector2Int finalCoords;
    public enum DoorDirection
    {
        left, right, top, bottom
    }

    public DoorDirection doorDirection;
    public Door connection;
    [System.NonSerialized]
    public GameObject doorObject;
    private Tilemap doorTiles;
    private DoorTeleporter doorTeleporter;

    public bool isOpen = true;
    public bool isLocked = false;
    public bool isWall = false;

    void Start()
    {
        doorObject = GetComponentInChildren<SpriteRenderer>().gameObject;
        doorTiles = GetComponentInChildren<Tilemap>();
        doorTeleporter = doorObject.AddComponent<DoorTeleporter>();
        doorTeleporter.doorDirection = doorDirection;
    }

    public Door GetConnection()
    {
        return connection;
    }

    public void SetConnection(Door connection)
    {
        this.connection = connection;
        doorTeleporter.connection = connection;
    }

    /// <summary>
    /// Opens door by disabling tile colliders and changing sprite
    /// </summary>
    public void SetOpen()
    {
        if (!isWall)
        {
            isOpen = true;
            doorTeleporter.open = true;
            // doorObject.GetComponent<SpriteRenderer>().sprite = openSprite;
            
            TilemapCollider2D tiles = doorTiles.GetComponent<TilemapCollider2D>();
            if (tiles) tiles.enabled = false;
            BoxCollider doorCollider = doorObject.GetComponent<BoxCollider>();
            if (!doorCollider) doorCollider = doorObject.AddComponent<BoxCollider>();
            SpriteRenderer doorSprite = doorObject.GetComponent<SpriteRenderer>();
            if (doorSprite) doorSprite.enabled = false;
            doorCollider.isTrigger = true;
            doorCollider.size = new Vector2(0.2f, 0.1f);
            // doorCollider.edgeRadius = 0.3f;
        }
    }

    /// <summary>
    /// Closes door by enabling tile colliders and changing sprite
    /// </summary>
    public void SetClosed()
    {
        if (!isWall)
        {
            isOpen = false;
            doorTeleporter.open = false;
            // doorObject.GetComponent<SpriteRenderer>().sprite = closedSprite;
            
            TilemapCollider2D tiles = doorTiles.GetComponent<TilemapCollider2D>();
            if (tiles) tiles.enabled = true;

            SpriteRenderer doorSprite = doorObject.GetComponent<SpriteRenderer>();
            if (doorSprite) doorSprite.enabled = true;
        }
    }

    /// <summary>
    /// Sets this door to just a wall
    /// </summary>
    public void SetWall()
    {
        isWall = true;
        doorTeleporter.open = false;
        doorObject.SetActive(false);
        TilemapCollider2D tiles = doorTiles.GetComponent<TilemapCollider2D>();
        if (tiles) tiles.enabled = true;
    }

    public Room GetRight(int startX, int startZ)
    {
        return RoomController.instance.loadedRoomDict[new Vector2Int(startX + xOffset + 1, startZ + zOffset)];
    }

    public Room GetLeft(int startX, int startZ)
    {
        return RoomController.instance.loadedRoomDict[new Vector2Int(startX + xOffset - 1, startZ + zOffset)];
    }

    public Room GetTop(int startX, int startZ)
    {
        return RoomController.instance.loadedRoomDict[new Vector2Int(startX + xOffset, startZ + zOffset + 1)];
    }

    public Room GetBottom(int startX, int startZ)
    {
        return RoomController.instance.loadedRoomDict[new Vector2Int(startX + xOffset, startZ + zOffset - 1)];
    }
}
