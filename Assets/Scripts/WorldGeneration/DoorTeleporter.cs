using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorTeleporter : MonoBehaviour
{
    public Door.DoorDirection doorDirection;
    public Door connection;
    public bool open;

    void OnTriggerEnter(Collider other)
    {
        if (open)
        {
            if (other.tag == "Player" || other.tag == "Enemy")
            {
                Vector3 newPos = connection.doorObject.transform.position;
                switch (doorDirection)
                    {
                        case Door.DoorDirection.right:
                            newPos += new Vector3(3, 0, 0);
                            break;
                        case Door.DoorDirection.left:
                            newPos += new Vector3(-3, 0, 0);
                            break;
                        case Door.DoorDirection.top:
                            newPos += new Vector3(0, 0, 3);
                            break;
                        case Door.DoorDirection.bottom:
                            newPos += new Vector3(0, 0, -3);
                            break;
                    }
                other.attachedRigidbody.transform.position = newPos;
            }
        }
    }
}
