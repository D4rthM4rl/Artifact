using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargingEnemy : Enemy
{
    private bool charging = false;
    [System.NonSerialized]
    public float maxSpeed = 0;
    protected int timeSeeing = 0;
    
    [SerializeField]
    private float chargeSpeedMultiplier = 3f;
    [SerializeField]
    private float chargeAcceleration = 100f;
    [SerializeField]
    private float minChargingSpeed = 3f;
    [SerializeField]
    private float speedPerMoveSpeed = 0.1f;
    [SerializeField]
    private float baseStopSpeed = 5f;
    [SerializeField]
    private float stopSpeedPerMoveSpeed = 0.01f;
    [SerializeField]
    private int requiredSightFrames = 100;

    void Start()
    {
        EnemyStart();
    }

    void FixedUpdate()
    {
        if (currState == CharacterState.inactive) return;
        // Debug.Log(currState);
        EnemyUpdate();
        focusPos = focus.transform.position;
        if (Vector3.Distance(focusPos, transform.position) <= meleeRange * attackSizeModifier && willAttack.Contains(focus) && !cooldownAttack)
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
            HitCharacter(focus.GetComponent<Character>(), (meleeDamage * attackDamageModifier));
        }
    }

    public override void HitCharacter(Character other, float damage)
    {
        other.TakeDamage(meleeDamage, false);
        other.ReceiveEffect(attackEffects);
        Vector3 knockbackDirection = (other.transform.position - transform.position).normalized;

        // Apply knockback force to what I hit
        Rigidbody otherRb = other.gameObject.GetComponent<Rigidbody>();
        otherRb.AddForce(knockbackDirection * knockbackModifier + Mathf.Pow(knockbackModifier, 1.5f) * rb.velocity, ForceMode.Impulse);
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
			ai.speed = moveSpeed * 3;
			ai.acceleration = 100;
			ai.autoBraking = false;
			// Debug.Log("Max speed: " + maxSpeed);
			// Debug.Log("Curr speed: " + rb.velocity.magnitude);
			Vector3 toDest = ai.destination - transform.position;
			toDest.y = 0;
			// Debug.DrawLine(transform.position, (transform.position + toDest).normalized * range, Color.black);
			if ((rb.velocity.magnitude < (1 + 0.1 * moveSpeed) && maxSpeed > (5 + 0.01 * moveSpeed)) ||
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
			ai.speed = moveSpeed;
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
    protected override void Wander()
    {
        
    }
}
