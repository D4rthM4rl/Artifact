using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponItemController : ItemController
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<SpriteRenderer>().sprite = item.itemImage;
        Destroy(GetComponent<PolygonCollider2D>());
        PolygonCollider2D collider = gameObject.AddComponent<PolygonCollider2D>();
        collider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<Player>()) 
        {
            GameObject obj = this.gameObject;
            Player player = collision.gameObject.GetComponent<Player>();
            
            player.AddWeapon(obj);

            Destroy(obj);
            // obj.SetActive(false);
        }
    }
}
