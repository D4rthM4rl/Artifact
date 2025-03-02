using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupernovaController : Weapon
{
    //Getting Rigid body, defining mousePos for use in angling the weapon at the mouse cursor
    SpriteRenderer spi;
    public Rigidbody2D rb;
    public Vector2 mousePos;

    private Transform currLoc;
    private float currSize = 0;

    private float canFire = 1;
    private Transform firepoint;
    private Transform holdPoint;

    public float fireRate;
    public float bulletSpeed = 20f;
    public float bulletLifetime;
    public GameObject particleEffect; // Reference to the particle effect prefab

    //Getting sprite renderer
    void Start()
    {
        holdPoint = GameObject.Find("HoldPivot").transform;
    }

    void Update()
    {
        // transform.position = holdPoint.transform.position;
        fireRate = user.attackRateModifier;
        if (Input.GetButton("Fire1") && isSelected && Time.time > canFire && user.UseMana(manaUse))
        {
            Vector2 target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = Vector2.zero;
			if (user is Player) 
			{target = Camera.main.ScreenToWorldPoint(Input.mousePosition);}
			else 
			{direction = (user as Enemy).focusPos;}
            StartCoroutine(Fire(target));
            canFire = Time.time + fireRate;
        }
    }

    /// <summary>
    /// Start supernova effect at mouse
    /// </summary>
    /// <param name="pos">Position to start supernova</param>
    IEnumerator Fire(Vector2 pos)
    {
        inUse = true;
        float currentSize = 0f;

        // Loop until duration is reached
        float timer = 0f;
        GameObject particles = Instantiate(particleEffect, pos, Quaternion.identity) as GameObject;
        SupernovaParticles particlesScript = particles.GetComponent<SupernovaParticles>();
        particlesScript.damage = 0.25f * user.attackDamageModifier;
        particlesScript.knockback = user.knockbackModifier;
        particlesScript.size = user.attackSizeModifier;
        float gravity = 5f;
        while (timer < 3f)
        {
            // Increase the size gradually
            currentSize += 2 * Time.deltaTime;
            currentSize = Mathf.Clamp(currentSize, 0f, size);

            // Increment timer
            timer += Time.deltaTime;
            particles.GetComponent<ParticleSystemForceField>().gravity = gravity;
            gravity += 0.01f;

            yield return null;
        }

        timer = 0f;
        while (timer < 1f)
        {
            gravity -= 0.2f;
            particles.GetComponent<SupernovaParticles>().exploded = true;
            particles.GetComponent<ParticleSystemForceField>().gravity = gravity;
            yield return null;
            timer += Time.deltaTime;
        }
        Destroy(particles);

        inUse = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        if (currLoc != null) {
            Gizmos.DrawWireSphere(currLoc.position, currSize);
        } else {
            Gizmos.DrawWireSphere(transform.position, currSize);
        }
    }

    void FixedUpdate() 
    {
        //Setting mousePos to the mouse location as a vector 2, and subtracting the rigid body position,
        //vector math, taking arctan of line to determine angle in radians, correcting for error with -180
        // mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 180;
        // rb.rotation = angle;

        // if (angle >= -90 - 52.5 && angle <= -52)
        // {
        //     spi.sortingOrder = -5;
        // }
        // else 
        // {
        //     spi.sortingOrder = 5;
        // }

        // //flipping sprite over y axis when cursor is on left half of screen to maintain proper orientation
        // if (angle <= -90 && angle >= -270)
        // {
        //     spi.flipY = true;
        // }
        // else
        // {
        //     spi.flipY = false;
        // }
    }
}