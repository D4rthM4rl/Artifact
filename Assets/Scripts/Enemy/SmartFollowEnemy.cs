using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI;

public class SmartFollowEnemy : Enemy
{
    protected override void Start() {base.Start();}

	protected override void Update() {base.Update();}

    void FixedUpdate() {
        if (currState == CharacterState.inactive) return;
        focusPos = focus.transform.position;
        if (Vector3.Distance(focusPos, transform.position) <= meleeRange * stats.attackSizeModifier && !cooldownAttack && willAttack.Contains(focus))
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
        // TODO: Implement wandering behavior for SmartFollowEnemy
        if (ai != null && destinationRecalculate)
        {
            Vector3 randomPosition = transform.position + Random.insideUnitSphere * 5f;
            randomPosition.y = transform.position.y; // Keep same z position
            
            ai.destination = randomPosition;
            StartCoroutine(CooldownDestinationSet());
        }
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
            if (focus.GetComponent<Character>()) HitCharacter(focus.GetComponent<Character>(), meleeDamage * stats.attackDamageModifier);
            StartCoroutine(Cooldown());
        }
    }
}
