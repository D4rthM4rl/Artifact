using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI;

public class Steve : Enemy
{
    public GameObject bulletPrefab;

    [SerializeField]
    private float bulletSpawnOffset = 0.5f;

    protected override void Start() {Start();}

	protected override void Update() {base.Update();}

    void FixedUpdate() {
        if (currState == CharacterState.inactive) return;
        focusPos = focus.transform.position;
        if (!cooldownAttack && willAttack.Contains(focus))
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
        if (Vector3.Distance(focusPos, transform.position) <= meleeRange * AttackSizeModifier)
        {
            HitCharacter(focus.GetComponent<Character>(), meleeDamage * AttackDamageModifier);
        }
        else
        {
            Vector3 direction = (focusPos - transform.position).normalized;
            GameObject bulletObject = Instantiate(bulletPrefab, transform.position + direction * bulletSpawnOffset, Quaternion.identity) as GameObject;
            Projectile bullet = bulletObject.GetComponent<Projectile>();
            bullet.sender = this;
            bullet.size = AttackSizeModifier;
            bullet.lifetime = ProjectileLifetimeModifier;
            bullet.knockback = KnockbackModifier / 10;
            bullet.canAttack = willAttack;
            Rigidbody bulletrb = bullet.GetComponent<Rigidbody>();
            bulletrb.AddForce(direction * ProjectileSpeedModifier, ForceMode.Impulse);   
        }
        StartCoroutine(Cooldown());
    }
}
