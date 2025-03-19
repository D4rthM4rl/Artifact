using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spring : SpecialTrait
{
    public float raycastDist = 0.55f;
    public float jumpForce = 10;

    // Update is called once per frame
    void Update()
    {
        LayerMask layerMask = LayerMask.GetMask("Environment");
        if (Input.GetButtonDown("Jump") && Physics.Raycast(gameObject.transform.position, Vector3.down, raycastDist, layerMask))
        {
            Debug.Log("JUmping");
            if (gameObject.GetComponent<Player>() != null)
            {
                gameObject.GetComponent<Rigidbody>().AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
        }
    }
}
