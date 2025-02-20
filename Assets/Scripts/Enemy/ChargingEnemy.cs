using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargingEnemy : Enemy
{
	[System.NonSerialized]
	public bool charging = false;
	[System.NonSerialized]
	public float maxSpeed = 0;
	[System.NonSerialized]
	public Vector2 chargingDir;
	protected int timeSeeing = 0;

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
        if (Vector2.Distance(focusPos, transform.position) <= meleeRange * attackSizeModifier && willAttack.Contains(focus) && !cooldownAttack)
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
		Vector2 knockbackDirection = (other.transform.position - transform.position).normalized;

		// Apply knockback force to what I hit
		Rigidbody2D otherRb = other.gameObject.GetComponent<Rigidbody2D>();
		otherRb.AddForce(knockbackDirection * knockbackModifier + Mathf.Pow(knockbackModifier, 1.5f) * rb.velocity, ForceMode2D.Impulse);
  	}

	protected override void Follow()
	{
		if (charging)
		{
			if (rb.velocity.magnitude < (3 + 0.1 * moveSpeed) && maxSpeed > (5 + 0.01 * moveSpeed))
			{
				charging = false;
				// Debug.Log("Stop");
			}
			timeSeeing = 0;
			if (chargingDir != (Vector2)(focusPos - transform.position))
			{
				chargingDir += 0.1f * (Vector2)(focusPos - transform.position);
			}
			rb.AddForce(chargingDir.normalized * moveSpeed * 0.5f, ForceMode2D.Impulse);
			maxSpeed = Mathf.Max(maxSpeed, rb.velocity.magnitude);
			// Debug.Log(rb.velocity.magnitude + " (" + rb.velocity.x + "," + rb.velocity.y + ")");
		}
		else
		{
			if (CanSeeTarget(focusPos, obstacleLayer))
			{
				if (timeSeeing > 100) 
				{
					maxSpeed = 0;
					charging = true;
					chargingDir = focusPos - transform.position;
				}
				else
				{
					timeSeeing++;
				}
			}
			else
			{
				timeSeeing = 0;
				targetPos = focusPos;
				MoveSmallTowardsPoint(targetPos, moveSpeed, ForceMode2D.Force);
			}
		}
	}

	protected override void Wander()
	{
		
	}
}
