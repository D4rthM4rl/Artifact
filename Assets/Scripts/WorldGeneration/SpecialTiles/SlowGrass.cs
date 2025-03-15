using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowGrass : SpecialTile<SlowGrass>
{
    private bool isRainSpawned = false;
    
    private void Awake()
    {
        // If spawned by rain, adjust appearance and lifetime
        WeatherSystem weatherSystem = FindObjectOfType<WeatherSystem>();
        if (weatherSystem != null && weatherSystem.currentWeather == WeatherType.Rain)
        {
            isRainSpawned = true;
            lifetime = 10f; // Longer lifetime for rain-spawned grass
            
            // Add a blue tint to indicate it's rain-created
            MeshRenderer renderer = GetComponentInChildren<MeshRenderer>();
            if (renderer != null)
            {
                Material material = renderer.material;
                material.color = new Color(0.7f, 0.7f, 1f, 0.8f);
            }
        }
    }
    
    protected override void ApplyEffect(GameObject gameObject, float originalSpeed)
    {
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        Character character = gameObject.GetComponent<Character>();
        if (character != null) 
        {
            // Rain-spawned slow grass has a stronger effect
            float multiplier = isRainSpawned ? 0.3f : 0.4f;
            character.moveSpeed = Mathf.Max(originalSpeed * .01f, character.moveSpeed * multiplier);
        }
    }

    protected override void UndoEffect(GameObject gameObject, float originalSpeed)
    {
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        Character character = gameObject.GetComponent<Character>();
        if (character != null) 
        {
            // Rain-spawned slow grass has a stronger effect
            float multiplier = isRainSpawned ? 0.3f : 0.4f;
            character.moveSpeed = Mathf.Min(originalSpeed, character.moveSpeed / multiplier);
        }
    }
}
