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

    private Vector2 lastPos;
    private Vector2 currPos;

    public List<Effect> effects = new List<Effect>();
    public HashSet<GameObject> canAttack;
    protected ContactFilter2D contactFilter;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DeathDelay());
        gameObject.layer = 11;

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
        transform.localScale = new Vector2(size, size);

        // Configure the ContactFilter2D
        contactFilter = new ContactFilter2D();
        contactFilter.useLayerMask = true; // Enable layer mask filtering

        // Set the layer mask to the layer(s) you want to check for overlap
        LayerMask mask = LayerMask.GetMask("Character", "Player", "Wall");
        contactFilter.SetLayerMask(mask);
    }

    void Update() {
        List<Collider2D> results = new List<Collider2D>();
		GetComponent<Collider2D>().OverlapCollider(contactFilter, results);
		foreach (Collider2D collider in results)
        {
			// Debug.Log(collider);
            if (collider.gameObject != null && (canAttack.Contains(collider.gameObject) || 
                (sender is Player && !collider.GetComponent<Player>())) || collider.gameObject.layer == 7)
            {
                gameObject.layer = 10;
            }
        }
    }

    /// <summary>Hit a character</summary>
    /// <param name="other">What character to hit</param>
    /// <param name="damage">How much damage to hit for</param>
    public virtual void HitCharacter(Character other, float damage)
    {
        other.TakeDamage(damage, false);
        other.ReceiveEffect(sender.attackEffects);
        other.attackedBy.Add(sender.species); 
        Vector2 knockbackDirection = (other.transform.position - transform.position).normalized;
        if (other is Enemy) StartCoroutine((other as Enemy).Alert(other.gameObject));

        // Apply knockback force to what I'm hitting
        Rigidbody2D targetRb = other.gameObject.GetComponent<Rigidbody2D>();
        targetRb.AddForce(knockbackDirection * sender.knockbackModifier, ForceMode2D.Impulse);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.tag == "Wall" || sender == null || sender.gameObject == null) 
        {
            Destroy(this.gameObject);
        }
        else if ((sender is Player || sender.willAttack.Contains(col.gameObject)) && sender.gameObject != col.gameObject && col.gameObject.GetComponent<Character>()) 
        {
            Character character = col.gameObject.GetComponent<Character>();
            HitCharacter(character, damage * sender.attackDamageModifier);

            Destroy(this.gameObject);
        }
        else
        {
            // Debug.Log("Ignoring collision with " + col.gameObject.tag);
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
