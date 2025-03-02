using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetalGust : ProjectileWeapon
{
    void FixedUpdate()
	{
		// transform.position = holdPoint.transform.position;
        float rate = cooldown * user.attackRateModifier;
        if (Input.GetButton("Fire1") && Time.time > canFire && isSelected && user.UseMana(manaUse))
        {
			Vector2 target = Vector2.zero;
			if (user is Player) 
			{
				target = (Camera.main.ScreenToWorldPoint(Input.mousePosition));
			}
			else 
			{
				target = ((user as Enemy).focusPos);
			}
            StartCoroutine(Fire(target));
            canFire = Time.time + rate;
        }
	}

    protected override IEnumerator Fire(Vector2 direction)
    {
        inUse = true;
		int numProjectiles = Random.Range(minNumProjectilesInFire, maxNumProjectilesInFire);
		for (int i = 0; i < numProjectiles; i++)
		{
			GameObject petal = Instantiate(projectilePrefab, firepoint.position, Quaternion.identity);
			
			SetupProjectile(petal.GetComponent<Projectile>());
            petal.GetComponent<PetalGustPetal>().target = direction;
			
			yield return new WaitForSeconds(timeBetweenProjectilesInFire);
		}
		inUse = false;
    }
}