using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowGrass : SpecialTile
{
    protected override void ApplyEffect(GameObject gameObject)
    {
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        float prevDrag = rb.drag;
        rb.drag = prevDrag + 3;
    }

    protected override void UndoEffect(GameObject gameObject)
    {
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        float prevDrag = rb.drag;
        rb.drag = prevDrag - 3;
    }
}
