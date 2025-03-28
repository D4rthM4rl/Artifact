using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WoxEnemy : CirclerEnemy
{  
    public GameObject bulletPrefab;
    public int teamsize = 0;
    [SerializeField]
    private float bulletSpawnOffset = 0.5f;

	protected override void Start() {base.Start();}

	protected override void Update() {base.Update();}

    void FixedUpdate()
    {
        if (currState == CharacterState.inactive) return;
        if (!CirclerController.teamEstablished) return;
        teamsize = CirclerController.teamTotal;
        if (CirclerController.anyAware)
        {
            awareOf = CirclerController.anyAwareOf;
            // A sound would be cool here
            // StartCoroutine(CirclerController.AlertAll());
            
        }

        focusPos = focus.transform.position;
        if (Vector3.Distance(transform.position, focusPos) <= CirclerController.circleRadius + 2 && !cooldownAttack && willAttack.Contains(focus))
        {
            currState = CharacterState.attack;
            Attack();
        }
        switch(currState)
        {
            case(CharacterState.inactive):
                return;
            case(CharacterState.wander): 
                Wander();
                break;
            case(CharacterState.follow):
                if (!followCooldown) StartCoroutine(FrequentFollow(0.05f));

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
    /// Attacks the target by firing a projectile in their direction
    /// </summary>
    protected override void Attack()
    {
        Vector3 direction = (focusPos - transform.position).normalized;
        GameObject bulletObject = Instantiate(bulletPrefab, transform.position + direction * bulletSpawnOffset, Quaternion.identity) as GameObject;
        Projectile bullet = bulletObject.GetComponent<Projectile>();
        bullet.sender = this;
        bullet.size = stats.attackSizeModifier;
        bullet.lifetime = stats.projectileLifetimeModifier * 2;
        bullet.knockback = stats.knockbackModifier;
        bullet.canAttack = willAttack;
        bullet.damage = 1;
        Rigidbody bulletrb = bullet.GetComponent<Rigidbody>();
        bulletrb.AddForce(direction * stats.projectileSpeedModifier, ForceMode.Impulse);
        StartCoroutine(Cooldown());
    }
}
