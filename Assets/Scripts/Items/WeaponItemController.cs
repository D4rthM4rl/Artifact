using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class WeaponItemController : MonoBehaviour
{
    public Item item;

    public GameObject weaponPrefab;

    // Start is called before the first frame update
    void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.sprite = item.itemImage;
        Destroy(GetComponent<Collider>());
        if (!gameObject.GetComponentInParent<Character>()){
            SphereCollider collider = gameObject.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            Light light = gameObject.AddComponent<Light>();
            light.color = Color.red;
            light.intensity = 30;
            light.range = 3;
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.GetComponent<Player>()) 
        {
            GameObject obj = this.gameObject;
            Light light = obj.GetComponent<Light>();
            if (light) 
            {
                Destroy(GetComponent<UniversalAdditionalLightData>());
                Destroy(light);
            }
            Player player = collision.gameObject.GetComponent<Player>();
            
            player.AddWeapon(obj);

            Destroy(obj);
            // obj.SetActive(false);
        }
    }
}
