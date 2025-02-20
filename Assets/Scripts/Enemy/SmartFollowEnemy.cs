using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartFollowEnemy : Enemy
{
    private void Start() {EnemyStart();}

    void FixedUpdate() {
        if (currState == CharacterState.inactive) return;
        EnemyUpdate();
        // focusPos = focus.GetComponent<Collider2D>().ClosestPoint(transform.position);
        focusPos = focus.transform.position;
        if (Vector2.Distance(focusPos, transform.position) <= meleeRange * attackSizeModifier && !cooldownAttack && willAttack.Contains(focus))
        {
            Attack();
            currState = CharacterState.attack;
        }
        switch(currState)
        {
            case(CharacterState.inactive):
                return;
            case(CharacterState.wander): 
                Wander();
                break;
            case(CharacterState.follow):
                Follow();
                break;
            case(CharacterState.flee):
                Flee();
                break;
            case(CharacterState.attack):
                // Keep doing attack thing
                break;
            case(CharacterState.die):
                Die();
                break;
        }
    }

    /// <summary>
    /// Wanders around randomly, not towards anything specific
    /// </summary>
    protected override void Wander()
    {
        // if (!chooseDir)
        // {   
        //     // If our seeded random isn't generated yet, we have to wait for it
        //     if (random == null) {random = GameController.seededRandom;}
        //     else {Debug.Log("Random found");StartCoroutine(ChooseDirection());}
        // }

        // // Apply force towards the direction
        // GetComponent<Rigidbody2D>().AddForce(transform.right * (speed * 0.2f));
    }

    /// <summary>Moves towards </summary>
    protected override void Follow()
    {
        // if (followDistance)
        targetPos = focusPos;
        MoveSmallTowardsPoint(targetPos, moveSpeed, ForceMode2D.Force);
    }

    /// <summary>
    /// Attacks towards player with whatever type of attack enemy has
    /// </summary>
    protected override void Attack()
    {
        if (!cooldownAttack) {
            if (focus.GetComponent<Character>()) HitCharacter(focus.GetComponent<Character>(), meleeDamage * attackDamageModifier);
            StartCoroutine(Cooldown());
        }
    }
}
