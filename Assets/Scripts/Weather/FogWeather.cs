using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Weather.asset", menuName = "WorldGenerationData/Weather/Fog")]

public class FogWeather : Weather
{
    public Color fogColor = new Color(0.76f, 0.76f, 0.76f, 1f);

    public float fogDensity = 0.02f;
    
    // Start fog - simplified to only use CameraController
    public override void StartWeather()
    {
        // currentWeather = WeatherType.Fog;
        
        if (CameraController.instance != null)
        {
            CameraController.instance.SetWeatherFog(fogColor, fogDensity, intensity);
            Debug.Log("Weather changed to fog");
        }
        else
        {
            Debug.LogWarning("CameraController not found. Cannot create fog effect.");
        }
    }

    // Stop current weather effect
    public override void StopWeather()
    {
        CameraController.instance.ClearWeatherFog();
        Debug.Log("Weather changed to sunny");
    }

	protected override void ChangeLighting()
	{
        WeatherController.instance.SetSkybox(skybox);
		RenderSettings.ambientIntensity = ambientBrightness;
        
        // Slightly tint the ambient color to be cooler/bluer
        RenderSettings.ambientLight = ambientColor;
        
        // Dim the sun if we found it
        if (WeatherController.instance.directionalLight != null)
        {
            WeatherController.instance.directionalLight.intensity = ambientBrightness;
            WeatherController.instance.directionalLight.color = ambientColor;
            WeatherController.instance.directionalLight.useColorTemperature = false;
        }
        
        Debug.Log("Fog light");
	}

	public override void ApplyEffectToCharacter(Character character)
	{
		// We don't add any effects
	}

	public override void ApplyEffectToAllCharacters()
	{
		// We don't add any effects
	}

	public override void RemoveEffectFromCharacter(Character character)
	{
		// We don't add any effects
	}

	public override void RemoveEffectFromAllCharacters()
	{
		// We don't add any effects
	}
}
