using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonCrawler
{
    public Vector2Int Position { get; set; }
    private System.Random random;
    /// <summary>
    /// Previous direction it went in as an int
    /// </summary>
    private int prevDirInt = -1;
    /// <summary>
    /// Tries not to retrace but with the way non 1x1 rooms generate, sometimes it still will
    /// </summary>
    private bool canRetrace;
    public DungeonCrawler(Vector2Int startPos, bool canRetrace, System.Random random)
    {
        Position = startPos;
        this.canRetrace = canRetrace;
        this.random = random;
    }

    /// <summary>
    /// Moves in a random direction (up/down/left/right)
    /// </summary>
    /// <param name="directionMovementMap">Map from direction (0-up, 1-left, etc) to Vector2Int.up, etc</param>
    /// <returns>The direction (Ex: Vector2Int.up) the crawler will move in</returns>
    public Vector2Int Move(Dictionary<Direction, Vector2Int> directionMovementMap)
    {
        int directionInt = random.Next(0, directionMovementMap.Count);
        while (canRetrace && prevDirInt == directionInt)
        {
            directionInt = random.Next(0, directionMovementMap.Count);
        }
        prevDirInt = directionInt;
        Position += directionMovementMap[(Direction) directionInt]; 

        return Position;
    }
}
