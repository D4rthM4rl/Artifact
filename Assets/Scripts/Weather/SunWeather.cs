using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Weather.asset", menuName = "WorldGenerationData/Weather/Sun")]

public class SunWeather : Weather
{

    public override void StartWeather()
	{
        ChangeLighting();
        ApplyEffectToAllCharacters();
	}

    // Stop current weather effect
    public override void StopWeather()
    {
        RemoveEffectFromAllCharacters();
        // Debug.Log("Weather changed from sunny");
    }

	protected override void ChangeLighting()
	{
        WeatherController.instance.SetSkybox(skybox);
        RenderSettings.ambientIntensity = skyboxBrightness;
        
        // Slightly tint the ambient color to be cooler/bluer
        RenderSettings.ambientLight = ambientColor;
        
        // Dim the sun if we found it
        if (WeatherController.instance.directionalLight != null)
        {
            WeatherController.instance.directionalLight.intensity = sunBrightness;
            WeatherController.instance.directionalLight.color = ambientColor;
            WeatherController.instance.directionalLight.useColorTemperature = true;
        }
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

    }
}
