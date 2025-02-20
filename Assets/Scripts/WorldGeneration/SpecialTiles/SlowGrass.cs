using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowGrass : SpecialTile
{
    protected override void ApplyEffect(GameObject gameObject)
    {
        Rigidbody2D rb = gameObject.GetComponent<Rigidbody2D>();
        float prevDrag = rb.drag;
        rb.drag = prevDrag + 3;
    }

    protected override void UndoEffect(GameObject gameObject)
    {
        Rigidbody2D rb = gameObject.GetComponent<Rigidbody2D>();
        float prevDrag = rb.drag;
        rb.drag = prevDrag - 3;
    }
}
