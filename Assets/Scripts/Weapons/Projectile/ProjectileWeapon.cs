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
        float rate = cooldown * user.attackRateModifier;
        if (Time.time > canFire && isSelected && user.UseMana(manaUse))
        {
			Vector3 direction = Vector3.zero;
			if (user is Player) 
			{
				if (Input.GetButton("Fire1"))
				{
					// Cast a ray from screen point
					Plane plane = new Plane(Vector3.back, transform.position);
					Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
					// RaycastHit hit;
					// // Hit ground
					// if (Physics.Raycast(ray, out hit)) {
					// 	// Find the direction from the player to the hit point
					// 	direction = hit.point - transform.position;
					// }
					// else {
						// If you didn’t hit anything, just use the ray’s direction
						// Determine the point along the ray that hits the plane.
						if (plane.Raycast(ray, out float distance))
						{
							// Get the point where the ray intersects the plane.
							Vector3 hitPoint = ray.GetPoint(distance);

							// Determine the direction from the player to the hit point.
							direction = hitPoint - transform.position;
						}
					// }
					direction.Normalize();
					StartCoroutine(Fire(direction));
					canFire = Time.time + rate;
				} else {
					direction = Vector3.zero;
					if (Input.GetButton("LeftFire")) direction += Vector3.left;
					if (Input.GetButton("RightFire")) direction += Vector3.right;
					if (Input.GetButton("UpFire")) direction += Vector3.up;
					if (Input.GetButton("DownFire")) direction += Vector3.down;
					if (direction != Vector3.zero)
					{
						direction.Normalize();
						StartCoroutine(Fire(direction));
						canFire = Time.time + rate;
					}
				}
			}
			else 
			{
				direction = ((user as Enemy).focusPos - firepoint.position).normalized;
				StartCoroutine(Fire(direction));
				canFire = Time.time + rate;
			}
        }
	}

	protected virtual IEnumerator Fire(Vector3 direction)
    {
        inUse = true;
		int numProjectiles = Random.Range(minNumProjectilesInFire, maxNumProjectilesInFire + 1);
		for (int i = 0; i < numProjectiles; i++)
		{
			GameObject bullet = Instantiate(projectilePrefab, firepoint.position, Quaternion.identity, transform);
			
			SetupProjectile(bullet.GetComponent<Projectile>());
			
			Rigidbody bulletrb = bullet.GetComponent<Rigidbody>();
			bulletrb.AddForce(direction.normalized * projectileSpeed, ForceMode.Impulse);
			yield return new WaitForSeconds(timeBetweenProjectilesInFire);
		}
		inUse = false;
    }

	protected virtual void SetupProjectile(Projectile projectile)
	{
        projectile.sender = user;
        projectile.size = size * user.attackSizeModifier;
        projectile.damage = damage * user.attackDamageModifier;
        projectile.knockback = knockback * user.knockbackModifier;
		projectile.speed = projectileSpeed * user.projectileSpeedModifier;
		projectile.lifetime = projectileLifetime * user.projectileLifetimeModifier;
		projectile.canAttack = user.willAttack;
		if (projectile.gameObject != null) projectile.gameObject.layer = user.gameObject.layer + 1;
        foreach (Effect e in user.attackEffects) {projectile.effects.Add(EffectController.instance.GetEffect(e));}
	}

	public void PWStart() {Start();}

	public void PWFixedUpdate() {FixedUpdate();}
}