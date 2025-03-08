using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfinityItem : SpecialTrait
{
    public float pushBackForce = .001f; // Factor by which to slow down enemies and bullets
    public float slowdownRadius = 2f; // Distance within which enemies and bullets are slowed down
    public Material material;
    public Character user;
    private ParticleSystem particles;

    void Start()
    {
        particles = gameObject.GetComponent<ParticleSystem>();
        if (particles == null)
        {
            particles = gameObject.AddComponent<ParticleSystem>();
        }
        var e = particles.emission;
        e.rateOverTime = 0;
        // // var shape = e.shape;
        // e.radius = slowdownRadius / 2;
        var main = particles.main;
        // main.duration = 25f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.0f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.3f, 1f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.2f);
        Gradient ourGradient = new Gradient();
        ourGradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(new Color(0.0f, 0.1f, 1), 0f), new GradientColorKey(new Color(0.2f, 0, 1), 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) }
        );
        // Couldn't get Gradient to work
        var psRenderer = GetComponent<ParticleSystemRenderer>();
        psRenderer.material = GameController.instance.particleMaterial;
        psRenderer.sortingOrder = 1;
        main.startColor = ourGradient;
        user = gameObject.GetComponent<Character>();
    }

    void Update()
    {
        var e = particles.emission;

        if (user.mana > 0.01) {
            Collider[] colliders = Physics.OverlapSphere(transform.position, slowdownRadius);
            foreach (Collider col in colliders)
            {
                // Check if the collider belongs to an enemy or a bullet
                if (col.tag == "Enemy")
                {
                    e.rateOverTime = 16;
                    user.UseMana(0.01f);
                    // Slow down the enemy or bullet
                    Rigidbody rb = col.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        Vector3 knockbackDirection = -(transform.position - rb.transform.position).normalized;
                        // Could unnormalize it I think, so that it's stronger the closer you get as an upgrade
                        rb.AddForce(knockbackDirection * pushBackForce, ForceMode.Impulse);
                    }
                    // If the object doesn't have a Rigidbody, may need to handle slowdown differently
                }
            }
        }
        int pRate = (int) Mathf.Round(e.rateOverTime.constant);
        if (pRate >= 1)
        {
            e.rateOverTime = pRate - 2;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, slowdownRadius);
    }
}
