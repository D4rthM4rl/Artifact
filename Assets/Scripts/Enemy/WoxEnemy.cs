using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WoxEnemy : CirclerEnemy
{  
    public GameObject bulletPrefab;
    public int teamsize = 0;

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
        EnemyUpdate();
        focusPos = focus.transform.position;
        if (Vector2.Distance(transform.position, focusPos) <= CirclerController.circleRadius + 2 && !cooldownAttack && willAttack.Contains(focus))
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

    protected override void Attack()
    {
        // GameObject bullet = Instantiate(bulletPrefab, firepoint.position, firepoint.rotation);
        Vector2 direction = (focusPos - transform.position).normalized;
        GameObject bulletObject = Instantiate(bulletPrefab, transform.position + (Vector3)direction * 0.5f, Quaternion.identity) as GameObject;
        Projectile bullet = bulletObject.GetComponent<Projectile>();
        bullet.sender = this;
        bullet.size = attackSizeModifier;
        bullet.lifetime = 3;
        bullet.knockback = knockbackModifier;
        bullet.canAttack = willAttack;
        Rigidbody2D bulletrb = bullet.GetComponent<Rigidbody2D>();
        bulletrb.AddForce(direction * 5, ForceMode2D.Impulse);
        StartCoroutine(Cooldown());
    }
}
