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

    private List<GameObject> hiddenWalls = new List<GameObject>();

    void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        // Set an initial offset (for example, 5 units up and 12 units back)
        offset = new Vector3(0, 6, -16);

        // Initialize spherical coordinates based on the initial offset.
        currentDistance = offset.magnitude;
        currentElevation = Mathf.Asin(offset.y / currentDistance) * Mathf.Rad2Deg;
        currentAzimuth = Mathf.Atan2(offset.x, offset.z) * Mathf.Rad2Deg;
        
        // Store original fog settings
        StoreFogSettings();
    }

    /// <summary>
    /// Called when we start generating a new world
    /// </summary>
    public void StartWorldInitialization()
    {

    }

    /// <summary>
    /// Called when RoomGenerator is done generating rooms in new world
    /// </summary>
    public void FinishWorldInitialization()
    {
        if (currRoom != null && currRoom.isRegular)
        {
            UpdateWallVisibility();
        }
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
        GetCameraTargetPosition();
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

    /// <summary>
    /// Updates the offset (from the player)
    /// </summary>
    private void UpdateFreeCamOffset()
    {
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
            currentDistance = Mathf.Max(1f, currentDistance - 0.05f);
        }
        if (Input.GetButton("Zoom Out Cam"))
        {
            currentDistance = Mathf.Min(300, currentDistance + 0.05f);
        }

        // Recalculate offset based on the spherical coordinate values.
        float radElev = currentElevation * Mathf.Deg2Rad;
        float radAzimuth = currentAzimuth * Mathf.Deg2Rad;
        offset = new Vector3(
            currentDistance * Mathf.Sin(radAzimuth) * Mathf.Cos(radElev),
            currentDistance * Mathf.Sin(radElev),
            currentDistance * Mathf.Cos(radAzimuth) * Mathf.Cos(radElev)
        );
    }

    /// <summary>
    /// Updates the offset (from the room center) and updates the wall transparencies
    /// </summary>
    private void UpdateFixedRoomCamOffset()
    {
        if (Input.GetButtonDown("Move Cam Left"))
        {
            currentAzimuth -= 45f;
            RotateSpriteObjects(-45);
            UpdateWallVisibility();
        }
        if (Input.GetButtonDown("Move Cam Right"))
        {
            currentAzimuth += 45f;
            RotateSpriteObjects(45);
            UpdateWallVisibility();
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
            currentDistance = Mathf.Max(1f, currentDistance - 0.05f);
        }
        if (Input.GetButton("Zoom Out Cam"))
        {
            currentDistance = Mathf.Min(300, currentDistance + 0.05f);
        }
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
            if (sr.gameObject.CompareTag("Environment") || sr.gameObject.GetComponentInParent<Canvas>() // UI layer
             || sr.tag.Contains(" Wall"))
                continue;

            // Rotate the sprite to match the camera's Y rotation.
            // This means the sprite's front will rotate along with the camera.
            sr.transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y + degrees, 0);
        }
    }

    /// <summary>Hides (turns sprite renderers off) of walls that could be between player and camera</summary>
    private void UpdateWallVisibility()
    {
        if (currRoom == null) return;

        // Unhide previously hidden walls
        SetWallsVisibility(hiddenWalls, true);
        hiddenWalls.Clear();

        // Compute the horizontal direction from the azimuth
        float radAzimuth = currentAzimuth * Mathf.Deg2Rad;
        Vector2 horizontalDir2D = new Vector2(Mathf.Sin(radAzimuth), Mathf.Cos(radAzimuth));

        //Determine which walls to hide based on azimuth direction
        if (Mathf.Abs(horizontalDir2D.x) > 0.5f) // Threshold to allow corners
            hiddenWalls.AddRange(horizontalDir2D.x > 0 ? currRoom.rightWalls : currRoom.leftWalls);
        if (Mathf.Abs(horizontalDir2D.y) > 0.5f) // Threshold to allow corners
            hiddenWalls.AddRange(horizontalDir2D.y > 0 ? currRoom.topWalls : currRoom.bottomWalls);

        // Hide new walls
        SetWallsVisibility(hiddenWalls, false);
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

        // TODO: Make camera static in room if world is wall-separated and hide camera-side wall


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

    /// <summary>
    /// Find and set camera's angle and position of where it should be depending on
    /// whether we're in a room with walls and fixing camera to the room or follow player
    /// </summary>
    /// <returns>Where the camera should be/move towards</returns>
    private Vector3 GetCameraTargetPosition()
    {
        Vector3 targetPos;
        if (currRoom == null || !WorldGenerator.instance.worldGenerationData.wallsSeparateRooms) 
        {
            UpdateFreeCamOffset();
            targetPos = offset + player.transform.position;
            transform.position = targetPos;
            transform.LookAt(player.transform.position);
        } 
        else if (currRoom.isRegular)
        {
            UpdateFixedRoomCamOffset();

            // Get the center of the room
            Vector3 roomCenter = currRoom.GetRoomCenter();

            // Convert spherical coordinates (azimuth, elevation, distance) to Cartesian position
            float radAzimuth = currentAzimuth * Mathf.Deg2Rad;
            float radElevation = currentElevation * Mathf.Deg2Rad;

            float horizontalDistance = currentDistance * Mathf.Cos(radElevation);
            float yOffset = currentDistance * Mathf.Sin(radElevation);

            // Calculate camera position
            targetPos = new Vector3(
                roomCenter.x + horizontalDistance * Mathf.Sin(radAzimuth),
                roomCenter.y + yOffset,
                roomCenter.z + horizontalDistance * Mathf.Cos(radAzimuth)
            );

            // Make the camera look at the room center (slightly above for a better view)
            Vector3 lookTarget = roomCenter + Vector3.up * 2f; // Adjust upward tilt as needed
            transform.position = targetPos;
            transform.LookAt(lookTarget);
        }
        else // If we're in a room and there are walls
        {
            // TODO: Make the camera clamp to walls
            UpdateFreeCamOffset();
            targetPos = offset + player.transform.position;
            transform.position = targetPos;
            transform.LookAt(player.transform.position);
        }

        return targetPos;
    }

    /// <summary>
    /// Enables or disables sprite renderers in a list of wall objects.
    /// </summary>
    private void SetWallsVisibility(List<GameObject> wallList, bool visible)
    {
        foreach (GameObject wall in wallList)
        {
            SpriteRenderer sr = wall.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.enabled = visible;
            }
        }
    }

    public void SetCurrRoom(Room room)
    {
        currRoom = room;
        UpdateWallVisibility();
    }

    public bool IsSwitchingScene()
    {
        return !transform.position.Equals(GetCameraTargetPosition());
    }

    /// <summary>
    /// Get the camera's current azimuth rotation in radians
    /// </summary>
    /// <returns>The current azimuth in radians</returns>
    public float GetAzimuthRadians()
    {
        return currentAzimuth * Mathf.Deg2Rad;
    }
    
    /// <summary>
    /// Get the camera's current azimuth rotation in degrees
    /// </summary>
    /// <returns>The current azimuth in degrees</returns>
    public float GetAzimuthDegrees()
    {
        return currentAzimuth;
    }
}
