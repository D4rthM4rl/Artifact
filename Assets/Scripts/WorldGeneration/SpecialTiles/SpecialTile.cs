using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SpecialTile : MonoBehaviour
{
    /// <summary>How many seconds this tile will exist</summary>
    public float lifetime = 3;

    /// <summary>What GameObjects are currently affected by my effect</summary>
    public HashSet<GameObject> affectedThings = new HashSet<GameObject>();

    /// <summary>Use blacklist if true, whitelist if false</summary>
    public bool useBlacklist = true;
    /// <summary>What GameObjects are vulnerable to my effect</summary>
    public HashSet<GameObject> whitelistedObjects = new HashSet<GameObject>();
    /// <summary>What GameObjects are not vulnerable to my effect</summary>
    public HashSet<GameObject> blacklistedObjects = new HashSet<GameObject>();

    private void Start() {
        Destroy(this.gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other) {
        if (other.GetComponent<Rigidbody>() == null) return;
        if (useBlacklist) {
            if (!blacklistedObjects.Contains(other.gameObject) && affectedThings.Add(other.gameObject)) ApplyEffect(other.gameObject);
        } else {
            if (whitelistedObjects.Contains(other.gameObject) && affectedThings.Add(other.gameObject)) ApplyEffect(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.GetComponent<Rigidbody>() == null) return;
        if (affectedThings.Remove(other.gameObject)) UndoEffect(other.gameObject);
    }

    protected abstract void ApplyEffect(GameObject gameObject);

    protected abstract void UndoEffect(GameObject gameObject);

    public void Remove() {
        Destroy(this.gameObject);
    }
}
