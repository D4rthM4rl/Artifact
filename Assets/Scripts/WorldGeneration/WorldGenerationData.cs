using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WorldGenerationData.asset", menuName = "WorldGenerationData/World Data")]

public class WorldGenerationData : ScriptableObject 
{
    public string worldName;
    /// <summary>
    /// How many crawlers move around the grid creating the map for the dungeon
    /// </summary>
    public int numberOfCrawlers;
    /// <summary>
    /// Whether a crawler can go in the opposite direction of the way it just went
    /// </summary>
    public bool canRetrace;
    /// <summary>
    /// How many steps each crawler can take at minimum
    /// </summary>
    public int iterationMin;
    /// <summary>
    /// How many steps each crawler can take at maximum
    /// </summary>
    public int iterationMax;
    /// <summary>
    /// The types of rooms that can be generated in the dungeon (randomly or not)
    /// </summary>
    public List<RoomType> roomTypes;
}

[System.Serializable]
public struct RoomType
{
    /// <summary>
    /// The type/name of the room (doesn't include world name)
    /// </summary>
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