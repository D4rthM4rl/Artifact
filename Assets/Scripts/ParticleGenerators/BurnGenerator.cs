using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurnGenerator : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        ParticleSystem ps = gameObject.AddComponent<ParticleSystem>();
        // var e = ps.emission;
        // e.rateOverTime = 0;
        // // // var shape = e.shape;
        // // e.radius = slowdownRadius / 2;
        var main = ps.main;
        // // main.duration = 25f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.0f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.1f, 2f);
        // main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.2f);
        Gradient ourGradient = new Gradient();
        ourGradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(new Color(1f, 0, 0), 0f), new GradientColorKey(new Color(1f, .3f, 0), 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) }
        );
        // // Couldn't get Gradient to work
        var psRenderer = GetComponent<ParticleSystemRenderer>();
        psRenderer.material = GameController.instance.particleMaterial;
        psRenderer.sortingOrder = 1;
        main.startColor = ourGradient;
    }

    void kys() {
        Destroy(this);
    }
}
