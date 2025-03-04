using System.Collections;
using UnityEngine;

public class GrassDash : Dash
{
    public GameObject grass;
    public float grassLifetime = 2f;

    private void Start() {
        grass = GameController.instance.slowGrass;
        cooldownTime = 0;
    }

    protected override void DoDash(Vector3 direction)
    {
        gameObject.GetComponent<Rigidbody>().AddForce(force * direction * 10 * Time.fixedDeltaTime, ForceMode.Impulse);
        SpawnGrasses();
    }

    private void SpawnGrasses()
    {
        if (Random.Range(0, 5) > 0) {
            SlowGrass grassTile = Instantiate(grass, gameObject.transform.position, Quaternion.identity).GetComponent<SlowGrass>();
            grassTile.blacklistedObjects.Add(gameObject);
            grassTile.lifetime = grassLifetime;
        }
    }
}
