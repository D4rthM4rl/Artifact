using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PetalGustPetal : Projectile
{
    //Getting Rigid body, defining mousePos for use in angling the weapon at the mouse cursor
    public Rigidbody rb;

    private Transform firepoint;
    private Transform holdPoint;

    public Vector2 target;
    
    // Start is called before the first frame update
    void Start()
    {
        ProjectileStart();
        rb = GetComponent<Rigidbody>();

        // Could use size here but it doesn't work that well
        transform.localScale = .75f * Vector3.one;

        Color c = Random.ColorHSV(0f, 1f, 1f, 1f, 1f, 1f);
        GetComponent<SpriteRenderer>().color = c;

        rb.AddForce(new Vector3((Random.Range(-8, 8)/ 10f), 5f) , ForceMode.Impulse);
        // rb.AddTorque(Random.Range(-40, 40), ForceMode.Impulse);
        // StartCoroutine(Fire(target));
    }

    // Start petals effect around player
    // IEnumerator Fire(Vector2 pos)
    // {
    //     // Gravity is strong until it reaches peak
    //     // rb.gravityScale = 3f; 2d implementation
    //     float globalGravity = -9.81f;

    //     Vector3 gravity = globalGravity * 3 * Vector3.up;
    //     rb.AddForce(gravity, ForceMode.Acceleration);
    //     while (rb.velocity.y >= 0) {yield return new WaitForSeconds(0.2f);}

    //     // Then it floats down slowly
    //     rb.gravityScale = 0.05f;
    //     yield return new WaitForSeconds(0.4f);

    //     rb.gravityScale = 0f;
    //     while (rb.velocity.y < -0.01)
    //     {
    //         rb.AddForce(new Vector2(0, 2f), ForceMode2D.Impulse);
    //         yield return new WaitForSeconds(0.05f);
    //     }
    //     // Calculate direction towards the player
    //     rb.velocity = Vector2.zero;
    //     rb.angularVelocity = 0;
    //     Vector2 direction = (pos - (Vector2)transform.position).normalized;
    //     float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 270;
    //     rb.rotation = angle;
    //     yield return new WaitForSeconds(0.1f);
        
        
    //     // Apply force towards the mouse
    //     rb.AddForce(direction * (speed * 0.35f), ForceMode2D.Impulse);
    // }

    IEnumerator DeathDelay()
    {
        yield return new WaitForSeconds(lifetime);
        Destroy(gameObject);
    }
}