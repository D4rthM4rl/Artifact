using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeatherType
{
    Sunny,
    Rain,
    Fog
}

public class WeatherSystem : MonoBehaviour
{
    public static WeatherSystem instance;

    // The prefabs for different weather systems
    public GameObject rainSystemPrefab;
    public GameObject fogSystemPrefab;
    
    // Currently active weather effect
    private GameObject activeWeatherEffect;
    
    // Current weather state
    public WeatherType currentWeather = WeatherType.Sunny;
    
    // Weather settings
    [Range(0f, 1f)]
    public float weatherIntensity = 0.7f;
    public float minWeatherDuration = 30f; // Minimum time a weather condition lasts
    public float maxWeatherDuration = 120f; // Maximum time a weather condition lasts
    
    // Weather probabilities (must sum to 1.0)
    [Range(0f, 1f)]
    public float rainProbability = 0.4f;
    [Range(0f, 1f)]
    public float fogProbability = 0.3f;
    // Sunny probability is calculated as 1 - (rainProbability + fogProbability)
    
    // Special tile effects during weather
    public GameObject slowGrassPrefab; // Prefab for SlowGrass tile
    public float specialTileSpawnChance = 0.05f; // Chance to spawn a special tile during rain

    // Fog settings
    public Color fogColor = new Color(0.76f, 0.76f, 0.76f, 1f);
    public float fogDensity = 0.02f;

    // Lighting settings
    [Range(0f, 1f)]
    public float rainLightIntensityMultiplier = 0.6f; // How dark it gets during rain
    private float originalAmbientIntensity;
    private Color originalAmbientColor;
    private Light directionalLight;
    private float originalLightIntensity;

    // Rain effects on characters
    /// <summary>Characters have their speed multiplied by this in the rain</summary>
    [Range(0.5f, 1.5f)]
    public float rainSpeedModifier = 1.1f;
    /// <summary>Characters have their drag multiplied by this in the rain</summary>
    [Range(0.3f, 1.3f)]
    public float rainDragModifier = 0.7f; 
    private List<Character> affectedCharacters = new List<Character>();
    private float checkInterval = 3f; // Check for new characters every 2 seconds
    private float lastCheckTime = 0f;
    
    private void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Initialize the weather system
    public void Initialize()
    {
        // Store original lighting settings
        StoreOriginalLightSettings();
        
        // Start the weather changing coroutine
        StartCoroutine(RandomWeatherChange());
    }

    // Store original lighting settings
    private void StoreOriginalLightSettings()
    {
        originalAmbientIntensity = RenderSettings.ambientIntensity;
        originalAmbientColor = RenderSettings.ambientLight;
        
        // Find the main directional light (usually the sun)
        directionalLight = FindObjectOfType<Light>();
        if (directionalLight != null)
        {
            originalLightIntensity = directionalLight.intensity;
        }
    }
    
    // Coroutine to change weather randomly over time
    private IEnumerator RandomWeatherChange()
    {
        while (true)
        {
            float weatherDuration = Random.Range(minWeatherDuration, maxWeatherDuration);
            yield return new WaitForSeconds(weatherDuration);
            
            // Choose a new weather type
            ChangeToRandomWeather();
        }
    }
    
    // Change to a randomly selected weather based on probabilities
    public void ChangeToRandomWeather()
    {
        // Clear current weather first
        ClearCurrentWeather();
        
        float random = Random.value;
        if (random < rainProbability)
        {
            SetWeather(WeatherType.Rain);
        }
        else if (random < rainProbability + fogProbability)
        {
            SetWeather(WeatherType.Fog);
        }
        else
        {
            SetWeather(WeatherType.Sunny);
        }
    }
    
    /// <summary>
    /// Sets the weather to the given type
    /// </summary>
    /// <param name="weatherType">The type of weather to set to</param>
    public void SetWeather(WeatherType weatherType)
    {
        ClearCurrentWeather();
        currentWeather = weatherType;
        
        switch (weatherType)
        {
            case WeatherType.Rain:
                StartRain();
                break;
            case WeatherType.Fog:
                StartFog();
                break;
            case WeatherType.Sunny:
                // Nothing to do for sunny weather
                Debug.Log("Weather changed to sunny");
                break;
        }
    }
    
    // Clear any active weather effects
    private void ClearCurrentWeather()
    {
        StopAllCoroutines();
        
        if (activeWeatherEffect != null)
        {
            Destroy(activeWeatherEffect);
            activeWeatherEffect = null;
        }
        
        // Clear fog if it was active
        if (currentWeather == WeatherType.Fog && CameraController.instance != null)
        {
            CameraController.instance.ClearWeatherFog();
        }
        
        // Remove rain effects from all characters if it was raining
        if (currentWeather == WeatherType.Rain)
        {
            RemoveRainEffectFromAllCharacters();
        }
        
        // Restore original lighting
        RestoreLighting();
    }

    // Dim the lighting for rain effect
    private void DimLightingForRain()
    {
        // Reduce ambient light intensity
        RenderSettings.ambientIntensity = originalAmbientIntensity * rainLightIntensityMultiplier;
        
        // Slightly tint the ambient color to be cooler/bluer
        Color rainAmbientColor = originalAmbientColor;
        rainAmbientColor.r *= 0.8f;
        rainAmbientColor.g *= 0.9f;
        RenderSettings.ambientLight = rainAmbientColor;
        
        // Dim the sun if we found it
        if (directionalLight != null)
        {
            directionalLight.intensity = originalLightIntensity * rainLightIntensityMultiplier;
        }
        
        Debug.Log("Dimmed lighting for rain effect");
    }
    
    // Restore original lighting
    private void RestoreLighting()
    {
        RenderSettings.ambientIntensity = originalAmbientIntensity;
        RenderSettings.ambientLight = originalAmbientColor;
        
        if (directionalLight != null)
        {
            directionalLight.intensity = originalLightIntensity;
        }
        
        Debug.Log("Restored original lighting");
    }
    
    // Start raining
    public void StartRain()
    {
        if (rainSystemPrefab != null)
        {
            print("Starting rain");
            currentWeather = WeatherType.Rain;
            activeWeatherEffect = Instantiate(rainSystemPrefab);
            
            // Set particle system intensity
            var particleSystems = activeWeatherEffect.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in particleSystems)
            {
                var emission = ps.emission;
                emission.rateOverTimeMultiplier = emission.rateOverTimeMultiplier * weatherIntensity;
            }
            
            // Dim the lighting during rain
            DimLightingForRain();

            // Start special tile effects
            StartCoroutine(SpawnSpecialTilesDuringRain());
            
            // Apply rain effect to all characters
            ApplyRainEffectToAllCharacters();
            
            Debug.Log("Weather changed to rain");
        }
    }
    
    // Start fog - simplified to only use CameraController
    public void StartFog()
    {
        currentWeather = WeatherType.Fog;
        
        if (CameraController.instance != null)
        {
            CameraController.instance.SetWeatherFog(fogColor, fogDensity, weatherIntensity);
            Debug.Log("Weather changed to fog");
        }
        else
        {
            Debug.LogWarning("CameraController not found. Cannot create fog effect.");
        }
    }
    
    // Stop current weather effect
    public void StopWeather()
    {
        ClearCurrentWeather();
        currentWeather = WeatherType.Sunny;
        Debug.Log("Weather changed to sunny");
    }
    
    // Spawns special tiles during rain
    private IEnumerator SpawnSpecialTilesDuringRain()
    {
        while (currentWeather == WeatherType.Rain)
        {
            yield return new WaitForSeconds(5f); // Check every 5 seconds
            
            if (slowGrassPrefab != null && Random.value < specialTileSpawnChance)
            {
                Room currentRoom = RoomController.instance.currRoom;
                if (currentRoom != null)
                {
                    // Calculate a random position within the current room
                    Vector3 roomPosition = currentRoom.transform.position;
                    float roomSizeX = RoomController.xWidth;
                    float roomSizeZ = RoomController.zWidth;
                    
                    Vector3 randomPosition = new Vector3(
                        roomPosition.x + Random.Range(-roomSizeX/2, roomSizeX/2),
                        roomPosition.y + 0.1f, // Slightly above the ground
                        roomPosition.z + Random.Range(-roomSizeZ/2, roomSizeZ/2)
                    );
                    
                    // Spawn the special tile
                    Instantiate(slowGrassPrefab, randomPosition, Quaternion.identity);
                }
            }
        }
    }

    // Apply rain effect to all characters
    private void ApplyRainEffectToAllCharacters()
    {
        Character[] characters = FindObjectsOfType<Character>();
        foreach (Character character in characters)
        {
            if (!affectedCharacters.Contains(character))
            {
                // Apply rain speed modifier
                character.ChangeMoveSpeed(rainSpeedModifier, true);
                Rigidbody rb = character.GetComponent<Rigidbody>();
                if (rb) rb.drag = rb.drag * rainDragModifier;
                affectedCharacters.Add(character);
            }
        }
    }
    
    // Remove rain effect from all affected characters
    private void RemoveRainEffectFromAllCharacters()
    {
        foreach (Character character in affectedCharacters)
        {
            if (character != null) // Check if character still exists
            {
                // Reverse the rain speed modifier
                character.ChangeMoveSpeed(1f / rainSpeedModifier, true);
                Rigidbody rb = character.GetComponent<Rigidbody>();
                if (rb) rb.drag = rb.drag / rainDragModifier;
            }
        }
        affectedCharacters.Clear();
    }

    private void Update()
    {
        // Periodically check for new characters during rain
        if (currentWeather == WeatherType.Rain && Time.time > lastCheckTime + checkInterval)
        {
            ApplyRainEffectToAllCharacters();
            lastCheckTime = Time.time;
        }
    }
}
