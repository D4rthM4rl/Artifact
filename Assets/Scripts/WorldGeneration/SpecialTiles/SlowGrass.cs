using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowGrass : SpecialTile<SlowGrass>
{
    private bool rainSpawned = false;
    
    private void Awake()
    {
        // Make it green like normal
        MeshRenderer renderer = GetComponentInChildren<MeshRenderer>();
        if (renderer != null)
        {
            Material material = renderer.material;
            material.color = new Color(0.2f, 0.6f, .1f, 0.8f);
        }
    }
    
    protected override void ApplyEffect(GameObject gameObject, float originalSpeed)
    {
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        Character character = gameObject.GetComponent<Character>();
        if (character != null) 
        {
            float multiplier = 0.4f;
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
            float multiplier = 0.4f;
            character.moveSpeed = Mathf.Min(originalSpeed, character.moveSpeed / multiplier);
        }
    }
}
