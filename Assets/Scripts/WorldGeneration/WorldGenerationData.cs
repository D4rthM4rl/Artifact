using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "WorldGenerationData.asset", menuName = "WorldGenerationData/World Data")]

public class WorldGenerationData : ScriptableObject 
{
    public string worldName;
    /// <summary> How many crawlers move around the grid creating the map for the dungeon </summary>
    [Range(0, 10)]
    public int numberOfCrawlers;
    /// <summary> Whether a crawler can go in the opposite direction of the way it just went </summary>
    public bool canRetrace;
    /// <summary> How many steps each crawler can take at minimum </summary>
    [Range(0, 100)]
    public int iterationMin;
    /// <summary> How many steps each crawler can take at maximum </summary>
    [Range(0, 100)]
    public int iterationMax;
    /// <summary>How units wide a 1x1 coordinate room is in the x</summary> 
    public int xRoomWidth;
    /// <summary>How units wide a 1x1 coordinate room is in the z</summary>
    public int zRoomWidth;

    /// <summary>How much the y-level of the room changes as you move a room closer to the end
    // (probably wouldn't work as well if the end isn't guaranteed to be the farthest room)</summary>
    public float yChangeTowardsEnd;

    /// <summary>Whether the rooms are separated by walls or not</summary>
    public bool wallsSeparateRooms;

    /// <summary> The types of rooms that can be generated in the dungeon (randomly or not)</summary>
    public List<RoomType> roomTypes;

    /// <summary> The types of decorations that will be randomly placed generated in the dungeon randomly</summary>
    public List<Decoration> randomDecorations;
}

[System.Serializable]
public struct RoomType
{
    /// <summary> The type/name of the room (doesn't include world name) </summary>
    public string type;
    /// <summary>
    /// How common the room is, higher number relative to other weights means more common
    /// </summary>
    public int weight;
    /// <summary>
    /// The coordinates the room takes up relative to its default starting coordinate
    /// </summary>
    public List<Vector2Int> coordinates;
}

/// <summary>Decorative things that may or may not be solid and randomly generate in rooms</summary>
[System.Serializable]
public struct Decoration
{
    /// <summary>What the decoration actually is</summary>
    public GameObject decoration;
    /// <summary>Whether it blocks enemies and stuff from spawning here</summary>
    public bool obstructing;

    public Vector3 size;

    /// <summary>How many clumps of this decoration minimum can be in the room</summary>
    [Tooltip("How many clumps of this decoration minimum can be in the room")]
    public int clumpTotalMin;
    [Tooltip("How many clumps of this decoration maximum can be in the room")]
    /// <summary>How many clumps of this decoration maximum can be in the room</summary>
    public int clumpTotalMax;

    /// <summary>How many of this decoration minimum can be in a clump</summary>
    [Tooltip("How many of this decoration minimum can be in a clump")]
    public int clumpSizeMin;
    /// <summary>How many of this decoration maximum can be in a clump</summary>
    [Tooltip("How many of this decoration maximum can be in a clump")]
    public int clumpSizeMax;
}