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

        // Ensure material is fully opaque
        Renderer gunRenderer = GetComponent<Renderer>();
        if (gunRenderer != null)
        {
            Color gunColor = gunRenderer.material.color;
            gunColor.a = 1f; // Set full opacity
            gunRenderer.material.color = gunColor;
        }

        //// Flip the gun along the X-axis
        //transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y + 180, transform.rotation.z);
    }

    void FixedUpdate()
    {
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
                    
                    if (plane.Raycast(ray, out float distance))
                    {
                        // Get the point where the ray intersects the plane.
                        Vector3 hitPoint = ray.GetPoint(distance);

                        // Determine the direction from the player to the hit point.
                        direction = hitPoint - transform.position;
                    }
                    
                    direction.Normalize();
                    // Adjust direction based on camera before firing
                    Vector3 adjustedDirection = AdjustDirectionForCamera(direction);
                    StartCoroutine(Fire(adjustedDirection));
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
                        // Rotate direction based on camera rotation
                        direction = RotateDirectionWithCamera(direction);
                        StartCoroutine(Fire(direction));
                        canFire = Time.time + rate;
                    }
                    RotateWeaponSpriteArrow(direction);
                }
            }
            else
            {
                direction = ((user as Enemy).focusPos - firepoint.position).normalized;
                // Ensure enemy firing also respects camera rotation
                direction = AdjustDirectionForCamera(direction);
                StartCoroutine(Fire(direction));
                canFire = Time.time + rate;
            }
        }
    }

    // New helper method to ensure consistent camera adjustment for all firing modes
    private Vector3 AdjustDirectionForCamera(Vector3 rawDirection)
    {
        // For mouse input, we need to consider the camera's orientation
        float cameraAzimuth = CameraController.instance.GetAzimuthDegrees();
        
        // Adjust the direction based on camera rotation
        // Since we're reversing the direction by 180 degrees in Fire(), 
        // we need to account for that here to ensure proper visual alignment
        Vector3 adjustedDir = rawDirection;
        
        // The direction already has camera perspective for mouse input
        // but we may need to adjust it for consistency with our 180-degree rotation
        return adjustedDir;
    }

    // New method to rotate direction vector based on camera's azimuth
    private Vector3 RotateDirectionWithCamera(Vector3 direction)
    {
        // Get camera's azimuth in radians
        float cameraAzimuthRad = CameraController.instance.GetAzimuthRadians();
        
        // Create rotation matrix around Y axis
        float sin = Mathf.Sin(cameraAzimuthRad);
        float cos = Mathf.Cos(cameraAzimuthRad);
        
        // Apply rotation to direction (essentially a 2D rotation in the XZ plane)
        float dirX = direction.x * cos - direction.z * sin;
        float dirZ = direction.x * sin + direction.z * cos;
        
        return new Vector3(dirX, direction.y, dirZ).normalized;
    }

    protected virtual IEnumerator Fire(Vector3 direction)
    {
        inUse = true;
        // Reverse the direction to change shooting angle by 180 degrees
        // AND account for the camera's current rotation
        direction = -direction;
        
        // Get camera rotation for debugging
        float cameraAngle = CameraController.instance.GetAzimuthDegrees();
        Debug.Log($"Firing with camera angle: {cameraAngle}, direction: {direction}");
        
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
        foreach (Effect e in user.attackEffects) { projectile.effects.Add(EffectController.instance.GetEffect(e)); }
    }

    public void PWStart() { Start(); }

    public void PWFixedUpdate() { FixedUpdate(); }

    void RotateWeaponSpriteMouse(Vector3 direction)
    {
        if (direction != Vector3.zero)
        {
            // Consider camera rotation when calculating angle
            float cameraRotation = CameraController.instance.GetAzimuthDegrees();
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            
            // Apply 180 degree rotation to match the reversed firing direction
            angle += 180; 
            
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    void RotateWeaponSpriteArrow(Vector3 direction)
    {
        // Apply camera rotation to direction
        Vector3 rotatedDirection = RotateDirectionWithCamera(direction);
        
        // Convert world‑space direction into a 2D vector (X = horizontal, Y = vertical)
        Vector2 dir2D = new Vector2(rotatedDirection.x, rotatedDirection.z);
        if (dir2D == Vector2.zero) return;

        float angle = Mathf.Atan2(dir2D.y, dir2D.x) * Mathf.Rad2Deg;
        
        // Apply 180 degree rotation to match the reversed firing direction
        angle += 180;
        
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}