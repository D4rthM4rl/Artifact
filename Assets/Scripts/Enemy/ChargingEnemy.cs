using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargingEnemy : Enemy
{
    private bool charging = false;
    [System.NonSerialized]
    public float maxSpeed = 0;
    protected int timeSeeing = 0;
	
	/// <summary>How much faster it charges than the normal move speed</summary>
    [SerializeField]
    private const float ChargeSpeedMultiplier = 3f;
	/// <summary>How fast it accelerates when charging</summary>
    [SerializeField]
    private const float ChargeAcceleration = 100f;
	/// <summary>Min speed to consider ending the "charge"</summary>
    [SerializeField]
    private const float MinChargingSpeed = 3f;

    [SerializeField]
    private const int RequiredSightFrames = 100;

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update() {
        base.Update();
    }

    void FixedUpdate()
    {
        if (currState == CharacterState.inactive) return;
        // Debug.Log(currState);
        focusPos = focus.transform.position;
        if (Vector3.Distance(focusPos, transform.position) <= meleeRange * stats.attackSizeModifier && willAttack.Contains(focus) && !cooldownAttack)
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
    
    protected override void Attack() 
    {
        if (!cooldownAttack) {
            StartCoroutine(Cooldown());
            HitCharacter(focus.GetComponent<Character>(), (stats.attackDamageModifier));
            Debug.Log("Attacking for " + stats.attackDamageModifier);
        }
    }

    public override void HitCharacter(Character other, float damage)
    {
        other.TakeDamage(damage, false);
        other.ReceiveEffect(attackEffects);
        Vector3 knockbackDirection = (other.transform.position - transform.position).normalized;

        // Apply knockback force to what I hit
        Rigidbody otherRb = other.gameObject.GetComponent<Rigidbody>();
        otherRb.AddForce(knockbackDirection * stats.knockbackModifier * KnockbackStart + Mathf.Pow(stats.knockbackModifier, 1.5f) * rb.velocity, ForceMode.Impulse);
    }

    protected override void Follow()
    {
        if (destinationRecalculate && !charging)
        {
            targetPos = focusPos;
            ai.destination = targetPos;
            StartCoroutine(CooldownDestinationSet());
        }


		if (charging)
		{
			ai.speed = stats.moveSpeed * ChargeSpeedMultiplier;
			ai.acceleration = ChargeAcceleration;
			ai.autoBraking = false;
			// Debug.Log("Max speed: " + maxSpeed);
			// Debug.Log("Curr speed: " + rb.velocity.magnitude);
			Vector3 toDest = ai.destination - transform.position;
			toDest.y = 0;
			// Debug.DrawLine(transform.position, (transform.position + toDest).normalized * range, Color.black);
			if ((rb.velocity.magnitude < (1 + 0.1 * stats.moveSpeed) && maxSpeed > (5 + 0.01 * stats.moveSpeed)) ||
			  Vector3.Distance(transform.position, ai.destination) < meleeRange && 
			  !Physics.Raycast(transform.position, toDest, range, 9))
			{
				charging = false;
				// Debug.Log("Stop");
			}
			timeSeeing = 0;
			maxSpeed = Mathf.Max(maxSpeed, rb.velocity.magnitude);
			// Debug.Log(rb.velocity.magnitude + " (" + rb.velocity.x + "," + rb.velocity.y + ")");
		}
		else
		{
			ai.speed = stats.moveSpeed;
			ai.acceleration = 50;
			if (CanSeeTarget(focusPos, obstacleLayer))
			{
				if (timeSeeing > 100)
				{
					maxSpeed = 0;
					charging = true;
					ai.SetDestination(focusPos);
				}
				else
				{
					timeSeeing++;
					// ai.SetDestination(focusPos);
				}
			}
			else
			{
				timeSeeing = 0;
				// ai.SetDestination(focusPos);
			}
		}
	}
}
