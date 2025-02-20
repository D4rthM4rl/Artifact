using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Friend.asset", menuName = "Friends/FriendObject")]
public class FriendData : ScriptableObject
{
    public string friendType;

    public float speed;
    public float damage;
    public float fireRate;
    public float bulletSpeed;
    public float bulletSize;
    public float bulletLifetime;
    public float knockback;
    public GameObject bulletPrefab;
}
