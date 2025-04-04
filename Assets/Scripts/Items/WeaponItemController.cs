using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class WeaponItemController : ItemController
{
    public WeaponStats weaponStats;

    public GameObject weaponPrefab;

    // Start is called before the first frame update
    void Start()
    {
        if (info == null) 
        {
            Debug.LogError("ItemStats is not set for " + gameObject.name);
            return;
        }
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.sprite = info.itemImage;
        if (GetComponent<Renderer>()) GetComponent<Renderer>().material.SetColor("_BaseColor", info.tint);
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
            if (!weaponStats.statsOnlyWhileUsing)
                {player.AddStatChange(info.statChanges);}

            Destroy(obj);
            // obj.SetActive(false);
        }
    }
}
