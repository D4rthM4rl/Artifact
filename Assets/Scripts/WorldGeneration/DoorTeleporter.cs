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
            if (other.gameObject.GetComponent<Character>())
            {
                Vector3 newPos = connection.doorObject.transform.position;
                switch (doorDirection)
                    {
                        // Decrease y by 2 because doors are slightly elevated
                        case Door.DoorDirection.right:
                            newPos += new Vector3(2, -2, 0);
                            break;
                        case Door.DoorDirection.left:
                            newPos += new Vector3(-2, -2, 0);
                            break;
                        case Door.DoorDirection.top:
                            newPos += new Vector3(0, -2, 2);
                            break;
                        case Door.DoorDirection.bottom:
                            newPos += new Vector3(0, -2, -2);
                            break;
                    }
                other.attachedRigidbody.transform.position = newPos;
            }
        }
    }
}
