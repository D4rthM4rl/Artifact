using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldTeleporter : MonoBehaviour
{
    public WorldGenerationData world;
    public bool open;

    void OnTriggerEnter(Collider other)
    {
        if (open)
        {
            if (other.tag == "Player")
            {
                //Need it to wait or we cause issues
                WorldGenerator.instance.worldGenerationData = this.world;
                RoomController.instance.UnloadAllRooms();
                // Stop any weather effects when leaving the world
                WeatherController.instance.StopWeather();
                GameObject.FindGameObjectWithTag("Player").transform.position = new Vector3(0, 0.5f, 0);
                WorldGenerator.instance.GenerateWorld();
            }
        }
    }
}
