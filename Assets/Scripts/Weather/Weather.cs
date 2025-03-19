using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "Weather.asset", menuName = "WorldGenerationData/Weather")]

public abstract class Weather : ScriptableObject 
{
    /// <summary>The name of this specific weather type</summary>
    public string weatherName;
    /// <summary>How intense is the weather</summary>
    [Range(0.1f, 10)]
    [Tooltip("How intense is the weather")]
    public int intensity;

    /// <summary>How long the weather lasts minimum (in sec)</summary>
    [Tooltip("How long the weather lasts minimum (in sec)")]
    public int minDuration = 90;
    /// <summary>How long the weather lasts maximum (in sec)</summary> 
    [Tooltip("How long the weather lasts maximum (in sec)")]
    public int maxDuration = 240;

    /// <summary>What other special traits through scripting the weather has</summary>
    [Tooltip("What other special traits/scripts the weather has")]
    public List<SpecialWeatherBehavior> specialBehaviors;

    /// <summary>How bright the sun is, lighting up the world</summary>
    [Range(0, 2)]
    public float sunBrightness = 1;

    public Color sunColor = Color.yellow;

    /// <summary>What color the ambient light is, white by default</summary>
    public Color ambientColor = Color.white;

    /// <summary>What skybox to use when this weather is occuring</summary>
    public Material skybox;
    /// <summary>How bright the sky is, lighting up the world</summary>
    [Range(0, 2)]
    public float skyboxBrightness = 1;

    protected List<Character> affectedCharacters = new List<Character>();

    public abstract void StartWeather();

    protected abstract void ChangeLighting();

    public abstract void ApplyEffectToCharacter(Character character);

    public abstract void ApplyEffectToAllCharacters();

    public abstract void RemoveEffectFromCharacter(Character character);
    
    public abstract void RemoveEffectFromAllCharacters();

    public abstract void StopWeather();
}