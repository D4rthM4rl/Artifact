using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SpecialTile<T> : MonoBehaviour where T : SpecialTile<T>
{
    /// <summary>How many seconds this tile will exist</summary>
    public float lifetime = 3;

    /// <summary>What GameObjects are currently affected by my effect</summary>
    public static Dictionary<GameObject, float> affectedThings = new Dictionary<GameObject, float>();

    /// <summary>Use blacklist if true, whitelist if false</summary>
    public bool useBlacklist = true;
    /// <summary>What GameObjects are vulnerable to my effect</summary>
    public HashSet<GameObject> whitelistedObjects = new HashSet<GameObject>();
    /// <summary>What GameObjects are not vulnerable to my effect</summary>
    public HashSet<GameObject> blacklistedObjects = new HashSet<GameObject>();

    private void Start() 
    {
        StartCoroutine(Remove(lifetime));
    }

    private void OnTriggerEnter(Collider other) 
    {
        if (other.GetComponent<Rigidbody>() == null) return;
        float speed = 0;
        if (other.gameObject.GetComponent<Character>() != null) speed = other.gameObject.GetComponent<Character>().MoveSpeed;
        if (useBlacklist) {
            if (!blacklistedObjects.Contains(other.gameObject) && !affectedThings.ContainsKey(other.gameObject)) 
            {
                affectedThings.Add(other.gameObject, speed);
                ApplyEffect(other.gameObject, speed);
            }
        } else {
            if (whitelistedObjects.Contains(other.gameObject) && !affectedThings.ContainsKey(other.gameObject)) 
            {
                affectedThings.Add(other.gameObject, speed);
                ApplyEffect(other.gameObject, speed);
            }
        }
    }

    private void OnTriggerExit(Collider other) 
    {
        if (other.GetComponent<Rigidbody>() == null) return;
        if (affectedThings.ContainsKey(other.gameObject)) 
        {
            UndoEffect(other.gameObject, affectedThings[other.gameObject]);
            affectedThings.Remove(other.gameObject);
        }
    }

    public IEnumerator Remove(float lifetime) 
    {
        yield return new WaitForSeconds(lifetime);
        foreach (GameObject go in affectedThings.Keys)
        {
            if (go != null) UndoEffect(go, affectedThings[go]);
        }
        Destroy(this.gameObject);
    }

    protected abstract void ApplyEffect(GameObject gameObject, float originalStat);

    protected abstract void UndoEffect(GameObject gameObject, float originalStat);

}
