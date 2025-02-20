using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupernovaParticles : MonoBehaviour
{
    public ParticleSystem system;
    public List<ParticleCollisionEvent> collisionEvents;
    public float damage;
    public float knockback;
    public float size;

    public bool exploded = false;

    void Start()
    {
        system = GetComponent<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>();
    }

    void Update()
    {
        if (!exploded)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 1 + size);
            foreach (Collider2D col in colliders)
            {
                // Check if the collider belongs to an enemy or a bullet
                if (col.CompareTag("Enemy") || col.CompareTag("Player"))
                {   // Knockback enemy towards center of it
                    Vector2 direction = (transform.position - col.transform.position).normalized;

                    // Apply knockback force to the enemy
                    Rigidbody2D enemyRb = col.GetComponent<Rigidbody2D>();
                    if (enemyRb != null)
                    {
                        enemyRb.AddForce(direction * knockback * 0.015f, ForceMode2D.Impulse);
                    }
                }
            }
        }
    }

    void OnParticleCollision(GameObject other)
    {
        int numCollisionEvents = system.GetCollisionEvents(other, collisionEvents);

        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();

        int i = 0;

        while (i < numCollisionEvents)
        {
            if (other.tag == "Player")
            {
                Vector3 pos = collisionEvents[i].intersection;
                Vector2 force;
                if (exploded) {force = collisionEvents[i].velocity * 20;}
                else {force = collisionEvents[i].velocity * 20;}
                rb.AddForce(force, ForceMode2D.Impulse);
            }
            else if (other.tag == "Enemy")
            {
                if (exploded) 
                {
                    Vector3 pos = collisionEvents[i].intersection;
                    Vector2 force = collisionEvents[i].velocity * knockback;
                    rb.AddForce(force);
                    other.gameObject.GetComponent<Enemy>().TakeDamage(damage);
                }
                else {other.gameObject.GetComponent<Enemy>().TakeDamage(damage * 0.1f);}
            }
            i++;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, size);
    }
}