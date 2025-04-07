using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character
{
    public bool paused;

    // Animator still communicates with your 3D model’s animation controller.
    Animator animator;

    public GameObject weapon;
    public GameObject[] weapons;
    private int numWeapons = 0;

    protected Rigidbody rb;

    // For 3D movement on XZ, we use a Vector3 (Y remains 0)
    private Vector3 movement;
    // Mouse world position determined via raycast
    private Vector3 mouseWorldPos;

    protected override void Start()
    {
        animator = GetComponent<Animator>();
        weapons = new GameObject[5];
        rb = GetComponent<Rigidbody>();
        base.Start();
        HealthUIController.instance.UpdateHearts(stats.health, stats.maxHealth);
    }

    void Update()
    {
        // If escape is pressed, pause the game by stopping time
        if (Input.GetButtonDown("Pause"))
        {
            Time.timeScale = paused ? 1 : 0;
            paused = !paused;
        }

        if (rb.velocity.y > 5f)  // Or whatever seems "too high"
        {
            Debug.LogWarning("Y-VELOCITY SPIKE: " + rb.velocity.y);
            Debug.Break(); // Pauses editor when this happens
        }

        // Get raw input from the player
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        // Get the camera's forward and right vectors and remove the vertical component.
        Vector3 camForward = Camera.main.transform.forward;
        camForward.y = 0;
        camForward.Normalize();

        Vector3 camRight = Camera.main.transform.right;
        camRight.y = 0;
        camRight.Normalize();

        // Create a camera-centric movement vector.
        movement = (camForward * verticalInput + camRight * horizontalInput).normalized;

        RegenMana();

        // Cast a ray from the screen point to determine the look direction.
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Vector3 dir;

        if (Physics.Raycast(ray, out hit))
        {
            // Determine direction from the player to the hit point.
            dir = hit.point - transform.position;
        }
        else
        {
            // If nothing is hit, use the ray's direction.
            dir = ray.direction;
        }

        // Set the speed parameter for the animator based on movement magnitude.
        animator.SetFloat("Speed", movement.magnitude);
    }

    void FixedUpdate()
    {
        // Apply movement physics.
        Vector3 movementForce = movement * stats.moveSpeed * 20 * Time.fixedDeltaTime;
        rb.AddForce(movementForce, ForceMode.Impulse);
        GameController.ApplyGravity(rb);

        // Determine look direction from player to mouse world position.
        float angle = 0;
        Plane plane = new Plane(Vector3.up, transform.position);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        if (plane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            Vector3 direction = hitPoint - transform.position;
            angle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
        }

        // Flip the sprite based on the calculated angle.
        if (angle >= -90 && angle <= 90)
        {
            GetComponent<SpriteRenderer>().flipX = true;
        }
        else
        {
            GetComponent<SpriteRenderer>().flipX = false;
        }
        
        animator.SetFloat("mouseang", angle);
    }

    #region Weapon Management

    public void AddWeapon(GameObject newWeapon)
    {
        if (numWeapons < HotbarUIController.instance.hotbarSize)
        {
            HotbarUIController.instance.weapons[numWeapons] = newWeapon.GetComponent<WeaponItemController>();

            // Instantiate the weapon as a child of the player’s rigidbody transform.
            weapon = Instantiate(newWeapon, rb.transform);
            weapon.transform.localPosition = new Vector3(.5f, 0, 0);
            weapon.layer = 8; // Player layer
            weapons[numWeapons] = weapon;

            // Remove the WeaponItemController script from the instantiated weapon.
            WeaponItemController itemController = weapon.GetComponent<WeaponItemController>();
            Destroy(itemController);

            // Enable all MonoBehaviour scripts attached to the weapon.
            Component[] components = weapon.GetComponents<Component>();
            foreach (Component comp in components)
            {
                MonoBehaviour script = comp as MonoBehaviour;
                if (script != null)
                {
                    script.enabled = true;
                }
            }

            // If the weapon has a renderer (e.g., MeshRenderer), disable it so that it can be shown later as needed.
            Renderer rend = weapon.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.enabled = false;
            }

            // Set the weapon’s user to this player.
            weapon.GetComponent<Weapon>().user = this;
            HotbarUIController.instance.DoHotbar();
            numWeapons++;
        }
        else
        {
            Debug.LogWarning("Too many weapons, add more spots");
        }
    }

    /// <summary>
    /// Changes selected weapon to new index if current one isn't in use for 0.1 sec or longer.
    /// </summary>
    /// <param name="index">Index to switch to (0 indexed)</param>
    public void ChangeWeapon(int index)
    {
        for (int i = 0; i < HotbarUIController.instance.hotbarSize; i++)
        {
            GameObject weaponGO = weapons[i];
            if (weaponGO)
            {
                Weapon weapon = weaponGO.GetComponent<Weapon>();
                if (weapon.inUse)
                {
                    // Buffer the input to keep the check going and if the weapon
                    // is still in use, don't switch
                    StartCoroutine(WaitFor(0.1f));
                    if (weapon.inUse)
                        return;
                }
                if (i == index)
                    weapon.SetSelected();
                else if (i != index)
                    weapon.SetUnselected();
            }
        }
    }

    #endregion

    #region Health and Mana

    public override void Die()
    {
        transform.position = Vector3.zero;
    }

    /// <summary>
    /// Heals the player.
    /// </summary>
    /// <param name="healAmount">Amount to heal</param>
    /// <param name="timeSpan">Time over which to heal</param>
    public override void Heal(float healAmount, float timeSpan)
    {
        if (timeSpan > 0)
        {
            float timePerUnit = timeSpan / healAmount;
            StartCoroutine(GradualHeal(timePerUnit, healAmount));
        }
        else
        {
            stats.health = Mathf.Min(stats.maxHealth, stats.health + healAmount);
            HealthUIController.instance.UpdateHearts(stats.health, stats.maxHealth);
        }
    }

    /// <summary>
    /// Gradually heals the player over time.
    /// </summary>
    /// <param name="timePerUnit">Time per healing unit (sec)</param>
    /// <param name="healAmount">Total healing amount</param>
    protected override IEnumerator GradualHeal(float timePerUnit, float healAmount)
    {
        stats.health = Mathf.Min(stats.maxHealth, stats.health + 1);
        UpdateHealth(stats.health, stats.maxHealth);
        for (int i = 0; i < healAmount - 1; i++)
        {
            yield return new WaitForSeconds(timePerUnit);
            stats.health = Mathf.Min(stats.maxHealth, stats.health + 1);
            UpdateHealth(stats.health, stats.maxHealth);
        }
    }

    protected override IEnumerator GradualHealMana(float timePerUnit, float healAmount)
    {
        UpdateMana(stats.mana);
        for (int i = 0; i < healAmount - 1; i++)
        {
            yield return new WaitForSeconds(timePerUnit);
            stats.mana = Mathf.Min(stats.mana + 1, 100);
            UpdateMana(stats.mana);
        }
    }

    protected override void UpdateMana(float mana)
    {
        ManaUIController.instance.UpdateManaUI(mana);
    }

    protected override void UpdateHealth(float health, float maxHealth)
    {
        HealthUIController.instance.UpdateHearts(health, maxHealth);
    }

    #endregion

    private IEnumerator WaitFor(float sec)
    {
        yield return new WaitForSeconds(sec);
    }
}
