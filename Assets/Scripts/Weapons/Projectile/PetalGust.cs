using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetalGust : ProjectileWeapon
{
    void FixedUpdate()
	{
		// transform.position = holdPoint.transform.position;
        float rate = cooldown * user.attackRateModifier;
        if (Time.time > canFire && isSelected && user.UseMana(manaUse))
        {
			Vector3 direction = Vector3.zero;
			if (Input.GetButton("LeftFire")) direction += Vector3.left;
			if (Input.GetButton("RightFire")) direction += Vector3.right;
			if (Input.GetButton("UpFire")) direction += Vector3.forward;
			if (Input.GetButton("DownFire")) direction += Vector3.back;
			if (direction != Vector3.zero)
			{
				direction.Normalize();
				StartCoroutine(Fire(direction));
				canFire = Time.time + rate;
			}
			Vector3 target = Vector3.zero;
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

    protected override IEnumerator Fire(Vector3 direction)
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