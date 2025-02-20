using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassiveEnemy : Enemy
{
    void FixedUpdate() {
        
    }

    /// <summary>
    /// Unused because won't attack, it's passive
    /// </summary>
    protected override void Attack()
    {
        throw new System.NotImplementedException();
    }

    protected override void Follow()
    {
        throw new System.NotImplementedException();
    }

    protected override void Wander()
    {
        
    }
}
