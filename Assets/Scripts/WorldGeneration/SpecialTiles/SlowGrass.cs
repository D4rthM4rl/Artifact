using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowGrass : SpecialTile<SlowGrass>
{
    private bool rainSpawned = false;
    
    private void Awake()
    {
        // Make it green like normal
        // MeshRenderer renderer = GetComponentInChildren<MeshRenderer>();
        // if (renderer != null)
        // {
        //     Material material = renderer.material;
        //     material.color = new Color(0.2f, 0.6f, .1f, 0.8f);
        // }
    }
    
    protected override void ApplyEffect(GameObject gameObject)
    {
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        Character character = gameObject.GetComponent<Character>();
        if (character != null) 
        {
            character.AddStatChange(statChanges);
        }
    }

    protected override void UndoEffect(GameObject gameObject)
    {
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        Character character = gameObject.GetComponent<Character>();
        if (character != null) 
        {
            character.RemoveStatChange(statChanges);
        }
    }
}
