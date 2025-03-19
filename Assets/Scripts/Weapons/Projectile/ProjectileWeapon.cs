using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ProjectileWeapon : Weapon
{
    public GameObject projectilePrefab;
    public Transform firepoint;
    protected Transform holdPoint;
    private SpriteRenderer sr;

    private float angle;

    public float projectileSpeed = 20f;
    public float projectileLifetime;
    public int minNumProjectilesInFire = 1;
    public int maxNumProjectilesInFire = 1;
    public float timeBetweenProjectilesInFire = 0;

    protected float canFire = 1;

    void Start()
    {
        holdPoint = user.holdPoint;

        // Ensure material is fully opaque
        Renderer gunRenderer = GetComponent<Renderer>();
        if (gunRenderer != null)
        {
            Color gunColor = gunRenderer.material.color;
            gunColor.a = 1f; // Set full opacity
            gunRenderer.material.color = gunColor;
        }
        sr = GetComponent<SpriteRenderer>();
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
                    RotateWeaponSpriteMouse(direction);
                }
                else
                {
                    direction = Vector3.zero;
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
                    RotateWeaponSpriteArrow(direction);
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
            GameObject bullet = Instantiate(projectilePrefab, firepoint.position, Quaternion.identity);

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
        foreach (Effect e in user.attackEffects) { projectile.effects.Add(EffectController.instance.GetEffect(e)); }
    }

    public void PWStart() { Start(); }

    public void PWFixedUpdate() { FixedUpdate(); }

    void RotateWeaponSpriteMouse(Vector3 direction)
    {
        if (direction != Vector3.zero)
        {
            angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg; // Convert to degrees

            if (angle >= -90f && angle < 0f)
            {
                transform.rotation = Quaternion.Euler(0, 0, angle);
                sr.flipX = true;
            }
            else if (angle >= 0f && angle < 90f)
            {
                transform.rotation = Quaternion.Euler(0, 0, angle);
                sr.flipX = true;
            }
            else if (angle >= 90f && angle < 180f)
            {
                transform.rotation = Quaternion.Euler(0, 0, 180f + angle);
                sr.flipX = false;
            }
            else
            {
                transform.rotation = Quaternion.Euler(0, 0, -180f + angle);
                sr.flipX = false;
            }
        }
    }

    void RotateWeaponSpriteArrow(Vector3 direction)
    {
        // Convert world‑space direction into a 2D vector (X = horizontal, Y = vertical)
        
        Vector2 dir2D = new Vector2(direction.x, direction.z);
        if (dir2D == Vector2.zero) return;

        float angle = Mathf.Atan2(dir2D.y, dir2D.x) * Mathf.Rad2Deg;

        if (angle >= -90f && angle < 0f)
        {
            transform.rotation = Quaternion.Euler(0, 0, angle);
            sr.flipX = true;
        }
        else if (angle >= 0f && angle < 90f)
        {
            transform.rotation = Quaternion.Euler(0, 0, -angle);
            sr.flipX = true;
        }
        else if (angle >= 90f && angle < 180f)
        {
            transform.rotation = Quaternion.Euler(0, 0, 180f + angle);
            sr.flipX = false;
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 0, -180f + angle);
            sr.flipX = false;
        }
    }
}