using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ProjectileWeapon : Weapon
{
	public GameObject projectilePrefab;
    public Transform firepoint;
    protected Transform holdPoint;

    public float projectileSpeed = 20f;
    public float projectileLifetime;
	public int minNumProjectilesInFire = 1;
	public int maxNumProjectilesInFire = 1;
	public float timeBetweenProjectilesInFire = 0;

	protected float canFire = 1;

	void Start()
    {
        holdPoint = user.holdPoint;
    }

	void FixedUpdate()
	{
		// transform.position = holdPoint.transform.position;
        float rate = stats.cooldown * user.attackRateModifier;
        if (Input.GetButton("Fire1") && Time.time > canFire && isSelected && user.UseMana(stats.manaUse))
        {
			Vector2 direction = Vector2.zero;
			if (user is Player) 
			{direction = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - firepoint.position).normalized;}
			else 
			{direction = ((user as Enemy).focusPos - firepoint.position).normalized;}
            StartCoroutine(Fire(direction));
            canFire = Time.time + rate;
        }
	}

	protected virtual IEnumerator Fire(Vector2 direction)
    {
        inUse = true;
		int numProjectiles = Random.Range(minNumProjectilesInFire, maxNumProjectilesInFire + 1);
		for (int i = 0; i < numProjectiles; i++)
		{
			GameObject bullet = Instantiate(projectilePrefab, firepoint.position, Quaternion.identity, transform);
			
			SetupProjectile(bullet.GetComponent<Projectile>());
			
			Rigidbody2D bulletrb = bullet.GetComponent<Rigidbody2D>();
			bulletrb.AddForce(direction.normalized * projectileSpeed, ForceMode2D.Impulse);
			yield return new WaitForSeconds(timeBetweenProjectilesInFire);
		}
		inUse = false;
    }

	protected virtual void SetupProjectile(Projectile projectile)
	{
        gameObject.layer = 11; // Intangible layer
        projectile.sender = user;
        projectile.size = stats.size * user.attackSizeModifier;
        projectile.damage = stats.damage * user.attackDamageModifier;
        projectile.knockback = stats.knockback * user.knockbackModifier;
		projectile.speed = projectileSpeed * user.projectileSpeedModifier;
		projectile.lifetime = projectileLifetime * user.projectileLifetimeModifier;
		projectile.canAttack = user.willAttack;
        foreach (Effect e in user.attackEffects) {projectile.effects.Add(EffectController.instance.GetEffect(e));}
	}

	public void PWStart() {Start();}

	public void PWFixedUpdate() {FixedUpdate();}
}
