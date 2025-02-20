using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    public static CameraController instance;
    public Room currRoom;
    public float moveSpeedOnRoomChange;
    private GameObject player;


    void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void LateUpdate()
    {
        UpdatePosition();
    }

    void UpdatePosition()
    {
        if (currRoom == null) {
            return;
        }

        Vector3 targetPos = GetCameraTargetPosition();

        transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * moveSpeedOnRoomChange);
        // transform.position = targetPos;
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
        return targetPos;
    }

    public bool IsSwitchingScene()
    {
        return !transform.position.Equals(GetCameraTargetPosition());
    }
}
