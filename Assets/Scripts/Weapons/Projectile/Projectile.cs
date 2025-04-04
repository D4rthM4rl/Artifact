using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    // These get changed by parent instantiating bullets
    public float lifetime;
    public float damage;
    public float size = 1f;
    public float speed;
    public float knockback;

    public Character sender;

    private Vector3 lastPos;
    private Vector3 currPos;

    public List<Effect> effects = new List<Effect>();
    public HashSet<GameObject> canAttack;
    LayerMask mask;
    SphereCollider col;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DeathDelay());
        // Layer should be setup by shooter in ProjectileWeapon

        foreach (Effect effect in effects)
        {
            GameObject p = effect.particles;
            
            // Component particleComp = instance.gameObject.AddComponent(p.GetType());
            GameObject particleObject = Instantiate(p, transform);
            ParticleSystem ps = particleObject.GetComponent<ParticleSystem>();
            var main = ps.main;
            main.startSize = 0.25f * size;
            var shape = ps.shape;
            shape.radius = 0.3f * size;
        }
        transform.localScale = new Vector3(size, size, size);

        // Set the layer mask to the layer(s) you want to check for overlap
        // mask = LayerMask.GetMask("Character", "Player", "Environment");
        col = gameObject.GetComponent<SphereCollider>();
        Physics.IgnoreCollision(col, sender.gameObject.GetComponent<Collider>());
    }

    void Update() {
        // int maxColliders = 10;
        // Collider[] results = new Collider[maxColliders];
        // Physics.OverlapSphereNonAlloc(col.center + transform.position, col.radius, results);
		// foreach (Collider collider in results)
        // {
        //     // Debug.Log("Projectile collision with " + collider.gameObject);
        //     // if (collider.gameObject.tag == "Environment" || sender == null || sender.gameObject == null) 
        //     // {
        //     //     Destroy(this.gameObject);
        //     // }
        //     // else if ((sender is Player || sender.willAttack.Contains(collider.gameObject)) && sender.gameObject != collider.gameObject && collider.gameObject.GetComponent<Character>()) 
        //     // {
        //     //     Character character = collider.gameObject.GetComponent<Character>();
        //     //     HitCharacter(character, damage * sender.attackDamageModifier);

        //     //     Destroy(this.gameObject);
        //     // }
        //     // else
        //     // {
        //     //     Debug.Log("Ignoring collision with " + collider.gameObject.tag);
        //     //     // Physics.IgnoreCollision(collider, col);
        //     // }
        //     if (collider == null || collider.gameObject == null || collider.gameObject == gameObject) continue;
		// 	Debug.Log(collider.gameObject);
        //     if ((canAttack.Contains(collider.gameObject) || (sender is Player && !collider.GetComponent<Player>()))
        //      || collider.gameObject.layer == 7)
        //     {
        //         gameObject.layer = 10;
        //     }
        // }
    }

    /// <summary>Hit a character</summary>
    /// <param name="other">What character to hit</param>
    /// <param name="damage">How much damage to hit for</param>
    public virtual void HitCharacter(Character other, float damage)
    {
        other.TakeDamage(damage, false);
        other.ReceiveEffect(sender.attackEffects);
        other.attackedBy.Add(sender.info.species); 
        Vector3 knockbackDirection = (other.transform.position - transform.position).normalized;
        if (other is Enemy) StartCoroutine((other as Enemy).Alert(other.gameObject));

        // Apply knockback force to what I'm hitting
        Rigidbody targetRb = other.gameObject.GetComponent<Rigidbody>();
        targetRb.AddForce(knockbackDirection * sender.stats.knockbackModifier, ForceMode.Impulse);
    }

    void OnCollisionEnter(Collision collision)
    {
        // Debug.Log("Projectile collision with " + collision.gameObject.tag);
        if (collision.gameObject.tag == "Environment" || collision.gameObject.tag.Contains("Wall") || 
          sender == null || sender.gameObject == null) 
        {
            Destroy(this.gameObject);
        }
        else if ((sender is Player || sender.willAttack.Contains(collision.gameObject)) && sender.gameObject != collision.gameObject && collision.gameObject.GetComponent<Character>()) 
        {
            Character character = collision.gameObject.GetComponent<Character>();
            HitCharacter(character, damage * sender.stats.attackDamageModifier);

            Destroy(this.gameObject);
        }
        else
        {
            // Debug.Log("Ignoring collision with " + collision.gameObject.tag);
            // Physics.IgnoreCollision(collision.collider, col);
        }
    }

    public void AddEffect(Effect e)
    {
        effects.Add(e);
    }

    IEnumerator DeathDelay()
    {
        yield return new WaitForSeconds(lifetime);
        Destroy(gameObject);
    }

    public void ProjectileStart() {Start();}
}
