using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using UnityEngine.Tilemaps;

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
    private SpriteRenderer doorSprite;
    private BoxCollider doorCollider;
    private DoorTeleporter doorTeleporter;

    public Color doorClosedColor;

    public bool isOpen = true;
    /// <summary>If door naturally opens on room clear or if it needs something special</summary>
    public bool isLocked = false;
    /// <summary>Whether the door is actually just a wall in this room, never opening</summary>
    public bool isWall = false;

    void Start()
    {
        doorClosedColor = WorldGenerator.instance.worldGenerationData.doorClosedColor;
        doorObject = gameObject;
        // doorTiles = GetComponentInChildren<Tilemap>();
        doorTeleporter = doorObject.AddComponent<DoorTeleporter>();
        doorTeleporter.doorDirection = doorDirection;
        doorCollider = doorObject.GetComponent<BoxCollider>();
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
    /// Opens door by removing or changing sprite and making collider a trigger
    /// </summary>
    public void SetOpen()
    {
        if (!isWall)
        {
            isOpen = true;
            doorTeleporter.open = true;
            // doorObject.GetComponent<SpriteRenderer>().sprite = openSprite;
            
            // TilemapCollider2D tiles = doorTiles.GetComponent<TilemapCollider2D>();
            // if (tiles) tiles.enabled = false;
            if (!doorCollider) Debug.LogError("Door " + gameObject + " needs collider");
            doorSprite = doorObject.GetComponent<SpriteRenderer>();
            if (doorSprite) doorSprite.enabled = false;
            doorCollider.isTrigger = true;
            // doorCollider.size = new Vector2(0.2f, 0.1f);
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
            doorObject.GetComponent<SpriteRenderer>().material.color = doorClosedColor;
            doorCollider.isTrigger = false;
            // doorObject.GetComponent<SpriteRenderer>().sprite = closedSprite;
            
            // TilemapCollider2D tiles = doorTiles.GetComponent<TilemapCollider2D>();
            // if (tiles) tiles.enabled = true;

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
        doorCollider.isTrigger = false;
        // doorObject.SetActive(false);
        // TilemapCollider2D tiles = doorTiles.GetComponent<TilemapCollider2D>();
        // if (tiles) tiles.enabled = true;
    }

    public Room GetRight(int startX, int startZ)
    {
        RoomController.instance.loadedRoomDict.TryGetValue(
            new Vector2Int(startX + xOffset + 1, startZ + zOffset), out Room room);
        return room;
    }

    public Room GetLeft(int startX, int startZ)
    {
        RoomController.instance.loadedRoomDict.TryGetValue(
            new Vector2Int(startX + xOffset - 1, startZ + zOffset), out Room room);
        return room;
    }

    public Room GetTop(int startX, int startZ)
    {
        RoomController.instance.loadedRoomDict.TryGetValue(
            new Vector2Int(startX + xOffset, startZ + zOffset + 1), out Room room);
        return room;
    }

    public Room GetBottom(int startX, int startZ)
    {
        RoomController.instance.loadedRoomDict.TryGetValue(
            new Vector2Int(startX + xOffset, startZ + zOffset - 1), out Room room);
        return room;
    }

}
