using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowGrass : SpecialTile<SlowGrass>
{
    protected override void ApplyEffect(GameObject gameObject, float originalSpeed)
    {
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        float prevDrag = rb.drag;
        // rb.drag = prevDrag + 3;
        Character character = gameObject.GetComponent<Character>();
        if (character != null) 
        {
            character.moveSpeed = Mathf.Max(originalSpeed * .01f, character.moveSpeed * 0.4f);
            // character.moveSpeed *= .5f;
            Debug.Log("Speed on enter: " + character.moveSpeed);
        }
    }

    protected override void UndoEffect(GameObject gameObject, float originalSpeed)
    {
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        float prevDrag = rb.drag;
        // rb.drag = prevDrag - 3;
        Character character = gameObject.GetComponent<Character>();
        if (character != null) 
        {
            character.moveSpeed = Mathf.Min(originalSpeed, character.moveSpeed / 0.4f);
            // character.moveSpeed /= .5f;
            Debug.Log("Speed on leave: " + character.moveSpeed);
        }
    }
}
