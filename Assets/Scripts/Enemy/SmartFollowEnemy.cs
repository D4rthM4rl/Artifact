using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI;

public class SmartFollowEnemy : Enemy
{
    private void Start() {EnemyStart();}

    void FixedUpdate() {
        if (currState == CharacterState.inactive) return;
        EnemyUpdate();
        focusPos = focus.transform.position;
        if (Vector3.Distance(focusPos, transform.position) <= meleeRange * attackSizeModifier && !cooldownAttack && willAttack.Contains(focus))
        {
            Attack();
            currState = CharacterState.attack;
        }
        switch(currState)
        {
            case(CharacterState.inactive):
            Follow();
                // return;
                break;
            case(CharacterState.wander): 
                Wander();
                break;
            case(CharacterState.follow):
                if (ai == null) break;
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

    /// <summary>Moves towards focus</summary>
    protected override void Follow()
    {
        // Debug.Log(ai.SetDestination(targetPos));
        if (destinationRecalculate)
        {
            targetPos = focusPos;
            ai.destination = targetPos;
            StartCoroutine(CooldownDestinationSet());
        }
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
