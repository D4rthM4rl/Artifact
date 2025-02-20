using UnityEngine;
using UnityEngine.Tilemaps;
 
[CreateAssetMenu(fileName = "New Tile Plus", menuName = "Tiles/Tile Plus")]
public class TilePlus : Tile
{
    public Sprite newSprite;
    public override void GetTileData(Vector3Int location, ITilemap tileMap, ref TileData tileData) {
        base.GetTileData(location, tileMap, ref tileData);
 
        tileData.sprite = newSprite;
    }
}