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

    /// <summary>Distance (zoom) from player cam is watching </summary>
    private float currentDistance;
    /// <summary>Vertical rotation angle in degrees</summary>
    private float currentElevation;
    /// <summary>Horizontal rotation angle in degrees</summary>
    private float currentAzimuth;
    
    private bool fogOn = false;
    private bool togglingFog = false;

    // Fog weather properties
    private Color originalFogColor;
    private float originalFogDensity;
    private bool originalFogState;
    private FogMode originalFogMode;

    public Material transparentMat;
    // Dictionary to store original materials so we can revert them later.
    private Dictionary<Renderer, Material> originalMats = new Dictionary<Renderer, Material>();

    void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        // Set an initial offset (for example, 5 units up and 12 units back)
        offset = new Vector3(0, 5, -12);

        // Initialize spherical coordinates based on the initial offset.
        currentDistance = offset.magnitude;
        currentElevation = Mathf.Asin(offset.y / currentDistance) * Mathf.Rad2Deg;
        currentAzimuth = Mathf.Atan2(offset.x, offset.z) * Mathf.Rad2Deg;
        
        // Store original fog settings
        StoreFogSettings();
    }

    // Store the original fog settings
    private void StoreFogSettings()
    {
        originalFogColor = RenderSettings.fogColor;
        originalFogDensity = RenderSettings.fogDensity;
        originalFogState = RenderSettings.fog;
        originalFogMode = RenderSettings.fogMode;
    }

    // LateUpdate is used so that camera movement occurs after player movement
    void LateUpdate()
    {
        UpdatePosition();
        HandleObstructions();
    }
    
    // Weather-based fog control methods
    public void SetWeatherFog(Color fogColor, float fogDensity, float intensity)
    {
        // Save original settings if not already saved
        if (!fogOn)
        {
            StoreFogSettings();
        }

        // Apply weather fog settings
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogDensity = fogDensity * Mathf.Clamp01(intensity);
        fogOn = true;
        
        // Log for debugging
        Debug.Log($"Weather fog set: Density={RenderSettings.fogDensity}, Intensity={intensity}");
    }
    
    public void ClearWeatherFog()
    {
        // Restore original fog settings
        RenderSettings.fogColor = originalFogColor;
        RenderSettings.fogDensity = originalFogDensity;
        RenderSettings.fogMode = originalFogMode;
        RenderSettings.fog = originalFogState;
        fogOn = false;
        
        Debug.Log("Weather fog cleared");
    }

    void UpdatePosition()
    {
        // if (currRoom == null) {
        //     return;
        // }

        // --- Handle Camera Input for Orbiting and Zooming ---
        // Rotate left/right by 45 degree increments.
        if (Input.GetButtonDown("Move Cam Left"))
        {
            currentAzimuth -= 45f;
            RotateSpriteObjects(-45);
        }
        if (Input.GetButtonDown("Move Cam Right"))
        {
            currentAzimuth += 45f;
            RotateSpriteObjects(45);
        }
        // Rotate up/down by 1 degree increments (clamped to avoid extreme angles).
        if (Input.GetButton("Move Cam Up"))
        {
            currentElevation = Mathf.Clamp(currentElevation + 0.1f, -89f, 89f);
        }
        if (Input.GetButton("Move Cam Down"))
        {
            currentElevation = Mathf.Clamp(currentElevation - 0.1f, -89f, 89f);
        }
        // Zoom in/out by changing the distance by 0.05 units at a time.
        if (Input.GetButton("Zoom In Cam"))
        {
            Debug.Log("Zooming in " + currentDistance);
            currentDistance = Mathf.Max(1f, currentDistance - 0.05f);
        }
        if (Input.GetButton("Zoom Out Cam"))
        {
            currentDistance = Mathf.Min(300, currentDistance + 0.05f);
            Debug.Log("Zooming out " + currentDistance);
        }

        // Recalculate offset based on the spherical coordinate values.
        float radElev = currentElevation * Mathf.Deg2Rad;
        float radAzimuth = currentAzimuth * Mathf.Deg2Rad;
        offset = new Vector3(
            currentDistance * Mathf.Sin(radAzimuth) * Mathf.Cos(radElev),
            currentDistance * Mathf.Sin(radElev),
            currentDistance * Mathf.Cos(radAzimuth) * Mathf.Cos(radElev)
        );

        // Remove manual fog toggle
        // If we wanted to use it by each room
        // Vector3 targetPos = GetCameraTargetPosition();
        // Vector3 targetPos = offset + new Vector3(player.transform.position.x, 0, player.transform.position.z);

        Vector3 targetPos = offset + player.transform.position;
        transform.position = targetPos;
        transform.LookAt(player.transform.position);
    }

    /// <summary>Rotates sprites to face camera when camera rotates</summary>
    private void RotateSpriteObjects(int degrees)
    {
        // Find all sprite renderers in the scene.
        SpriteRenderer[] spriteRenderers = FindObjectsOfType<SpriteRenderer>();

        // Iterate over each sprite.
        foreach (SpriteRenderer sr in spriteRenderers)
        {
            // Skip gameobjects tagged as "Wall" or "Floor"
            // if (sr.gameObject.CompareTag("Wall") || sr.gameObject.CompareTag("Floor"))
            if (sr.gameObject.CompareTag("Environment") || sr.gameObject.GetComponentInParent<Canvas>()) // UI layer
                continue;

            // Rotate the sprite to match the camera's Y rotation.
            // This means the sprite's front will rotate along with the camera.
            sr.transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y + degrees, 0);
        }
    }

    /// <summary>Makes objects between camera and player transparent</summary>
    void HandleObstructions()
    {
        if (player == null)
            return;

        // Get direction and distance from camera to player
        Vector3 direction = player.transform.position - transform.position;
        float distance = direction.magnitude;
        direction.Normalize();

        // Raycast between the camera and player
        Ray ray = new Ray(transform.position, direction);
        RaycastHit[] hits = Physics.RaycastAll(ray, distance);

        // List to store renderers currently obstructing the view
        List<Renderer> currentObstructions = new List<Renderer>();

        // Process each hit object.
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == null) continue;

            Renderer rend = hit.collider.GetComponent<Renderer>();
            if (rend != null && 
            hit.transform.gameObject != null 
            && hit.transform.GetComponent<Player>() == null
             && hit.transform.GetComponent<SpriteRenderer>() == null)
            {
                currentObstructions.Add(rend);
                // If not already modified, store the original color and adjust alpha.
                if (!originalMats.ContainsKey(rend))
                {
                    originalMats[rend] = rend.material;
                    Color newColor = rend.material.color;
                    rend.material = transparentMat;
                    newColor.a = 0.4f; // moderately transparent (adjust as needed)
                    rend.material.color = newColor;
                    
                    // Note: Some materials may need their shader changed to a transparent one.
                }
            }
        }

        // Revert any objects that are no longer obstructing the view.
        List<Renderer> renderersToRevert = new List<Renderer>();
        foreach (Renderer rend in originalMats.Keys)
        {
            if (!currentObstructions.Contains(rend))
            {
                renderersToRevert.Add(rend);
            }
        }
        foreach (Renderer rend in renderersToRevert)
        {
            if (rend != null)
            {
                rend.material = originalMats[rend];
            }
            originalMats.Remove(rend);
        }
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
