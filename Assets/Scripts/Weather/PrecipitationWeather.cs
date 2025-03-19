using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Weather.asset", menuName = "WorldGenerationData/Weather/Precipitation")]

public class PrecipitationWeather : Weather
{
    public float speedModifier = 1;

    public float dragModifier = 1;

    public Color precipitationColor = Color.white;

    [Range(0.3f, 3f)]
    public float particleSize = 0.6f;

    public float precipitationGravity = 1f;

    // Special tile effects during weather
    // public GameObject slowGrassPrefab; // Prefab for SlowGrass tile
    // public float specialTileSpawnChance = 0.05f; // Chance to spawn a special tile during rain
    
    // Start precipating
    public override void StartWeather()
    {
        Debug.Log("Starting rain");
        
        // Set particle system intensity
        ParticleSystem ps = WeatherController.instance.gameObject.GetComponent<ParticleSystem>();
        var emission = ps.emission;
        emission.rateOverTimeMultiplier = intensity * 100;
        emission.enabled = true;
        var psRenderer = WeatherController.instance.gameObject.GetComponent<ParticleSystemRenderer>();
        // psRenderer.material = GameController.instance.particleMaterial;
        psRenderer.material.color = precipitationColor;
        
        // Dim the lighting during rain
        ChangeLighting();

        // Start special tile effects
        // StartCoroutine(SpawnSpecialTilesDuringRain());
        
        // Apply rain effect to all characters
        ApplyEffectToAllCharacters();
        
        Debug.Log("Weather changed to rain");
    }

    // Dim the lighting for rain effect
    protected override void ChangeLighting()
    {
        WeatherController.instance.SetSkybox(skybox);
        // Reduce ambient light intensity
        RenderSettings.ambientIntensity = ambientBrightness;
        
        // Slightly tint the ambient color to be cooler/bluer
        // Color newAmbientColor = ambientColor;
        // newAmbientColor.r *= 0.8f;
        // newAmbientColor.g *= 0.9f;
        RenderSettings.ambientLight = ambientColor;
        
        // Dim the sun if we found it
        if (WeatherController.instance.directionalLight != null)
        {
            WeatherController.instance.directionalLight.intensity = ambientBrightness;
            WeatherController.instance.directionalLight.color = ambientColor;
            WeatherController.instance.directionalLight.useColorTemperature = false;
        }
        
        Debug.Log("Dimmed lighting for rain effect");
    }

	public override void ApplyEffectToCharacter(Character character)
	{
		throw new System.NotImplementedException();
	}

    // Apply rain effect to all characters
    public override void ApplyEffectToAllCharacters()
    {
        Character[] characters = FindObjectsOfType<Character>();
        foreach (Character character in characters)
        {
            if (!affectedCharacters.Contains(character))
            {
                // Apply rain speed modifier
                character.ChangeMoveSpeed(speedModifier, true);
                Rigidbody rb = character.GetComponent<Rigidbody>();
                if (rb) rb.drag = rb.drag * dragModifier;
                affectedCharacters.Add(character);
            }
        }
    }
    
	public override void RemoveEffectFromCharacter(Character character)
	{
		throw new System.NotImplementedException();
	}

    // Remove rain effect from all affected characters
    public override void RemoveEffectFromAllCharacters()
    {
        foreach (Character character in affectedCharacters)
        {
            if (character != null) // Check if character still exists
            {
                // Reverse the rain speed modifier
                character.ChangeMoveSpeed(1f / speedModifier, true);
                Rigidbody rb = character.GetComponent<Rigidbody>();
                if (rb) rb.drag = rb.drag / dragModifier;
            }
        }
        affectedCharacters.Clear();
    }

    // Spawns special tiles during rain
    private IEnumerator SpawnSpecialTilesDuringRain()
    {
        yield return new WaitForSeconds(5f); // Check every 5 seconds
        
        // if (slowGrassPrefab != null && Random.value < specialTileSpawnChance)
        // {
        //     Room currentRoom = RoomController.instance.currRoom;
        //     if (currentRoom != null)
        //     {
        //         // Calculate a random position within the current room
        //         Vector3 roomPosition = currentRoom.transform.position;
        //         float roomSizeX = RoomController.xWidth;
        //         float roomSizeZ = RoomController.zWidth;
                
        //         Vector3 randomPosition = new Vector3(
        //             roomPosition.x + Random.Range(-roomSizeX/2, roomSizeX/2),
        //             roomPosition.y + 0.1f, // Slightly above the ground
        //             roomPosition.z + Random.Range(-roomSizeZ/2, roomSizeZ/2)
        //         );
                
        //         // Spawn the special tile
        //         Instantiate(slowGrassPrefab, randomPosition, Quaternion.identity);
        //     }
        // }
    }

	public override void StopWeather()
	{
		ParticleSystem ps = WeatherController.instance.gameObject.GetComponent<ParticleSystem>();
        if (ps == null) ps = WeatherController.instance.gameObject.AddComponent<ParticleSystem>();
        var emission = ps.emission;
        emission.enabled = false;

        RemoveEffectFromAllCharacters();
	}
}
