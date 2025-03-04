using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Friend : MonoBehaviour
{
    private float canFire;
    private GameObject player;
    public FriendData friend;
    private float lastOffsetX;
    private float lastOffsetY;
    private Vector3 movement;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        // DOESN'T WORK FULLY BUT CLOSE ENOUGH I THINK WE CAN JUST CALL HIM KOOKY
        // Collecting movement inputs and using the abs value of movement inputs to trigger running animation.
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        Vector3 mousePos = (Camera.main.ScreenToWorldPoint(Input.mousePosition));
        
        float move = Mathf.Abs(movement.x) + Mathf.Abs(movement.y);
        // animator.SetFloat("Speed", move);
        
        if (Input.GetButton("Fire1") && Time.time > canFire)
        {
            Vector3 aim = mousePos - new Vector3(transform.position.x, transform.position.y);
            Fire(aim);
            canFire = Time.time + friend.fireRate;
        }

        if (movement.x != 0 && movement.y != 0)
        {
            float offsetX = (movement.x < 0) ? Mathf.Floor(movement.x) : Mathf.Ceil(movement.x);
            float offsetY = (movement.y < 0) ? Mathf.Floor(movement.y) : Mathf.Ceil(movement.y);
            // Calculate direction towards the player
            Vector3 direction = (player.transform.position - transform.position).normalized;
            // Apply force towards the player
            GetComponent<Rigidbody>().AddForce(direction * (friend.speed * 0.35f));
            lastOffsetX = offsetX;
            lastOffsetY = offsetY;
        }
        else {
            if (!(transform.position.x < lastOffsetX + 0.5f) || !(transform.position.y < lastOffsetY + 0.5f))
            {
                Vector3 direction = (new Vector3(player.transform.position.x - lastOffsetX, player.transform.position.y - lastOffsetY) - transform.position).normalized;
                GetComponent<Rigidbody2D>().AddForce(direction * (friend.speed * 0.35f), ForceMode2D.Impulse);
            }
        }
    }

    // Shoot gun
    void Fire(Vector3 aim)
    {
        GameObject bullet = Instantiate(friend.bulletPrefab, transform.position, transform.rotation);
        bullet.layer = 8; // Player attack layer
        Projectile bc = bullet.GetComponent<Projectile>();
        Player p = player.GetComponent<Player>();
        // foreach (Effect e in p.attackEffects) {bc.effects.Add(EffectController.instance.GetEffect(e));}
        bc.size = friend.bulletSize * p.attackSizeModifier;
        bc.damage = friend.damage * p.attackDamageModifier;
        bc.knockback = friend.knockback * p.knockbackModifier;
        Rigidbody2D bulletrb = bullet.GetComponent<Rigidbody2D>();
        bulletrb.AddForce(aim * friend.bulletSpeed, ForceMode2D.Impulse);
    }
}
