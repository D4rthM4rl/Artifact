using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    public static CameraController instance;
    public Room currRoom;
    public float moveSpeedOnRoomChange;
    private GameObject player;
    private Vector3 targetPos;
    private Vector3 offset;
    private bool fogOn = false;
    private bool togglingFog = false;

    void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        offset = new Vector3(0, 5, -12);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        UpdatePosition();
    }

    void UpdatePosition()
    {
        // if (currRoom == null) {
        //     return;
        // }

        // If we wanted to use it by each room
        // Vector3 targetPos = GetCameraTargetPosition();
        Vector3 targetPos = offset + new Vector3(player.transform.position.x, 0, player.transform.position.z);
        // Just to demonstrate fog
        if (Input.GetButton("Fire2") && !togglingFog)
        {
            StartCoroutine(toggleFog());
        } // Guns and stuff aren't occluded by fog because they don't have the right sprite material

        // transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * moveSpeedOnRoomChange);
        transform.position = targetPos;
        transform.LookAt(player.transform.position);
    }

    private IEnumerator toggleFog()
    {
        togglingFog = true;
        if (!fogOn)
        {
            while (RenderSettings.fogDensity < 0.15f)
            {
                fogOn = true;
                RenderSettings.fogDensity += 0.002f;
                yield return new WaitForSeconds(0.1f);
            }
        }
        else 
        {
            while (RenderSettings.fogDensity > 0)
            {
                fogOn = false;
                RenderSettings.fogDensity -= 0.002f;
                yield return new WaitForSeconds(0.1f);
            }
        }
        togglingFog = false;
    }

    Vector3 GetCameraTargetPosition()
    {
        Vector3 targetPos;
        if (currRoom == null) {
            return Vector3.zero;
        } else if (currRoom.isRegular)
        {
            targetPos = currRoom.GetRoomCenter();
            targetPos.z = transform.position.z;
        } else {
            targetPos = player.transform.position;
            targetPos.z = transform.position.z;
        }
        targetPos.y = 5;
        return targetPos;
    }

    public bool IsSwitchingScene()
    {
        return !transform.position.Equals(GetCameraTargetPosition());
    }
}
