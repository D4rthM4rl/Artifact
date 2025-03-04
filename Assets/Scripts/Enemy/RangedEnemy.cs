using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RangedEnemy : Enemy
{  
    public float bulletSpeed;
    public float bulletLifetime;
    public float bulletSize;
    public float rangeDamage;
    /// <summary>
    /// Bullet GameObject for enemy to shoot
    /// </summary>
    public GameObject bulletPrefab;

    protected override void Attack() {
        // // GameObject bullet = Instantiate(bulletPrefab, firepoint.position, firepoint.rotation);
        // Vector4 direction = (focusPos - transform.position).normalized;
        // GameObject bullet = Instantiate(bulletPrefab, transform.position + (Vector3)direction * 0.5f, Quaternion.identity) as GameObject;
        // bullet.tag = "Enemy";
        // bullet.layer = 10; // Enemy attack
        // BulletController bc = bullet.GetComponent<BulletController>();
        // // bc.GetPlayer(player.transform);
        // bc.damage = rangeDamage;
        // bc.size = bulletSize;
        // bc.lifetime = bulletLifetime;
        // bc.knockback = knockbackModifier;
        // bc.isEnemyBullet = true;
        // foreach (Effect e in attackEffects) {bc.effects.Add(e);}
        // Rigidbody2D bulletrb = bullet.GetComponent<Rigidbody2D>();
        // bulletrb.AddForce(direction * bulletSpeed, ForceMode2D.Impulse);
        // StartCoroutine(Cooldown());
    }
}
