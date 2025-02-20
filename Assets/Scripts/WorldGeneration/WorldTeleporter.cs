using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldTeleporter : MonoBehaviour
{
    public WorldGenerationData world;
    public bool open;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (open)
        {
            if (other.tag == "Player")
            {
                //Need it to wait or we cause issues
                WorldGenerator.instance.worldGenerationData = this.world;
                RoomController.instance.UnloadAllRooms();
                GameObject.FindGameObjectWithTag("Player").transform.position = Vector3.zero;
                WorldGenerator.instance.GenerateWorld();
            }
        }
    }
}
