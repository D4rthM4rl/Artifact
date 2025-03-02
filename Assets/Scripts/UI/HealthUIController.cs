using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthUIController : MonoBehaviour
{
    private static GameObject fullHeart; // Prefab of the heart UI element
    public static GameObject halfHeart;
    public static GameObject emptyHeart;
    private static Transform heartsParent; // Parent transform where hearts will be instantiated
    public static float maxHealth; // Maximum health
    public static float currentHealth; // Current health

    private void Awake()
    {
        if (fullHeart == null) {
            fullHeart = GameObject.Find("FullWhiteHeart");
        }
        if (halfHeart == null) {
            halfHeart = GameObject.Find("HalfWhiteHeart");
        }
        if (emptyHeart == null) {
            emptyHeart = GameObject.Find("EmptyHeart");
        }
        heartsParent = transform.parent.transform;
    }
    
    public static void UpdateHearts(float health, float max)
    {
        currentHealth = health;
        maxHealth = max;
        DoHearts();
    }

    // Function to update the hearts UI based on current health
    public static void DoHearts()
    {
        // Clear existing hearts
        foreach (Transform child in heartsParent)
        {
            if (child.name.Contains("Clone")){
                Destroy(child.gameObject);
            }
        }
        // Debug.Log("Doing hearts, " + currentHealth + " health out of " + maxHealth);
        // Instantiate hearts based on current health
        for (int i = 2; i <= maxHealth; i += 2)
        {
            float heartSpacing = 3f; // Adjust this value to control the spacing between hearts
            float heartSize = 15f;
            float xAdjust = heartSize + 15f;
            float yAdjust = -1 * (heartSize + 8f);
            // Calculate heart position
            float xPos = (i - 2) * (heartSpacing + heartSize);
            Vector3 heartPosition = new Vector3(xPos +  xAdjust, yAdjust, 0f);
            GameObject currHeart;
            if (Mathf.Ceil(currentHealth) >= i) {
                currHeart = fullHeart;
            } else if (Mathf.Ceil(currentHealth) == i - 1) {
                currHeart = halfHeart;
            } else {
                currHeart = emptyHeart;
            }
            // Instantiate heart
            GameObject heart = Instantiate(currHeart, heartsParent);
            SpriteRenderer spriteRenderer = heart.GetComponent<SpriteRenderer>();
            // If the SpriteRenderer component is not null (i.e., it exists), enable it
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
            }
            Transform heartRect = heart.GetComponent<Transform>();
            heartRect.localPosition = heartPosition;
            heartRect.localScale = new Vector2(heartSize, heartSize);
        }
    }
}
