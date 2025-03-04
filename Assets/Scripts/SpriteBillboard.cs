using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteBillboard : MonoBehaviour
{
    [SerializeField]
    bool lockXZ = true;
    void Update()
    {
        if (lockXZ)
        {
            transform.LookAt(Camera.main.transform);
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        }
        else
        {
            transform.LookAt(Camera.main.transform);
        }
    }
}
