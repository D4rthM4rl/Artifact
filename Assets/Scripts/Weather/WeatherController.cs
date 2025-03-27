using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherController : MonoBehaviour
{
    public static WeatherController instance;

    // The prefabs for different weather systems
    public GameObject rainSystemPrefab;
    public GameObject fogSystemPrefab;
    public GameObject snowSystemPrefab;
    
    // Current weather state
    public Weather currentWeather;
    public bool enableWeatherEffects;

    // Fog settings
    public Color fogColor = new Color(0.76f, 0.76f, 0.76f, 1f);
    public float fogDensity = 0.02f;

    // Lighting settings
    private float originalAmbientIntensity;
    private Color originalAmbientColor;
    public Light directionalLight;

    public Material defaultSkybox;
    
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

    private void Start()
    {
        InitializeWeatherSystem();
    }

    /// <summary>
    /// Initialize the weather system
    /// </summary>
    void InitializeWeatherSystem()
    {
        // Find or create the weather system
        if (enableWeatherEffects)
        {
            
            // Find the weather prefabs in Resources folder
            rainSystemPrefab = Resources.Load<GameObject>("RainSystem");
            fogSystemPrefab = Resources.Load<GameObject>("FogSystem");
            snowSystemPrefab = Resources.Load<GameObject>("SnowSystem");
            
            // Find the SlowGrass prefab in Resources folder
            // slowGrassPrefab = Resources.Load<GameObject>("SlowGrass");
            
            // Validate prefabs
            bool weatherPrefabsFound = rainSystemPrefab != null || 
                                       fogSystemPrefab != null ||
                                       snowSystemPrefab != null;
            
            if (!weatherPrefabsFound)
            {
                Debug.LogWarning("No weather system prefabs found in Resources folder. Weather effects will be disabled.");
                enableWeatherEffects = false;
            }    
        }
    }

    public void StartWeather()
    {
        currentWeather = DecideWeather();
        currentWeather.StartWeather();
    }

    private Weather DecideWeather()
    {
        int totalWeight = 0;
        List<LocalWeather> weathers = WorldGenerator.instance.worldGenerationData.weathers;
        foreach (LocalWeather weather in weathers)
        {
            totalWeight += weather.weight;
        }
        int randomNumber = GameController.seededRandom.Next(0, totalWeight);
        for (int i = 0; i < weathers.Count; i++)
        {
            if (randomNumber < weathers[i].weight)
            {
                return weathers[i].weather;
            }
            randomNumber -= weathers[i].weight;
        }
        Debug.LogError("Bad weather weight");
        return null;
    }

    // Clear any active weather effects
    public void StopWeather()
    {
        if (currentWeather != null) currentWeather.StopWeather();
    }

    // Initialize the weather system
    public void Initialize()
    {
        // Store original lighting settings
        // StoreOriginalLightSettings();
        
        // Start the weather changing coroutine
        // StartCoroutine(RandomWeatherChange());
    }

    // Store original lighting settings
    // private void StoreOriginalLightSettings()
    // {
    //     originalAmbientIntensity = RenderSettings.ambientIntensity;
    //     originalAmbientColor = RenderSettings.ambientLight;
        
    //     // Find the main directional light (usually the sun)
    //     directionalLight = FindObjectOfType<Light>();
    //     if (directionalLight != null)
    //     {
    //         originalLightIntensity = directionalLight.intensity;
    //     }
    // }
    
    // Coroutine to change weather randomly over time
    private IEnumerator RandomWeatherChange()
    {
        while (true)
        {
            // float weatherDuration = Random.Range(minWeatherDuration, maxWeatherDuration);
            // yield return new WaitForSeconds(weatherDuration);
            
            // // Choose a new weather type
            // ChangeToRandomWeather();
        }
    }


    private void Update()
    {
        // // Periodically check for new characters during rain or snow
        // if ((currentWeather == WeatherType.Rain || currentWeather == WeatherType.Snow) && 
        //     Time.time > lastCheckTime + checkInterval)
        // {
        //     if (currentWeather == WeatherType.Rain)
        //         ApplyRainEffectToAllCharacters();
        //     else if (currentWeather == WeatherType.Snow)
        //         ApplySnowEffectToAllCharacters();
                
        //     lastCheckTime = Time.time;
        // }
    }

    /// <summary>
    /// If the material isn't null, we 
    /// </summary>
    /// <param name="skyboxMat"></param>
    public void SetSkybox(Material skyboxMat)
    {
        if (skyboxMat != null) RenderSettings.skybox = skyboxMat;
        else RenderSettings.skybox = defaultSkybox;
    }
}
