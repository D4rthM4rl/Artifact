using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character
{
    // Animator still communicates with your 3D model’s animation controller.
    Animator animator;

    public GameObject weapon;
    public GameObject[] weapons;
    private short numWeapons = 0;

    public Rigidbody rb;

    // For 3D movement on XZ, we use a Vector3 (Y remains 0)
    private Vector3 movement;
    // Mouse world position determined via raycast
    private Vector3 mouseWorldPos;

    void Start()
    {
        animator = GetComponent<Animator>();
        weapons = new GameObject[5];
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Collect movement input: horizontal (x) and vertical (z) axes.
        movement = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

        // Convert mouse screen position into a world position on a horizontal plane.
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        // Here, the ground is assumed to be at y = 0. Adjust as necessary.
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float rayDistance;
        if (groundPlane.Raycast(ray, out rayDistance))
        {
            mouseWorldPos = ray.GetPoint(rayDistance);
        }

        // Use the movement magnitude to control a running animation.
        animator.SetFloat("Speed", movement.magnitude);
        RegenMana();
    }  

    void FixedUpdate()
    {
        // Apply movement physics.
        rb.AddForce(movement * moveSpeed * 20 * Time.fixedDeltaTime, ForceMode.Impulse);

        // Determine look direction from player to mouse world position.
        Vector3 lookDir = mouseWorldPos - transform.position;
        lookDir.y = 0; // Keep rotation strictly horizontal.
        if (lookDir.sqrMagnitude > 0.001f)
        {   
            // Optionally, compute an angle (in degrees) for animator parameters.
            float angle = Mathf.Atan2(lookDir.x, lookDir.z) * Mathf.Rad2Deg;
            animator.SetFloat("mouseang", angle);
        }
    }

    public void AddWeapon(GameObject newWeapon)
    {
        if (numWeapons < HotbarUIController.instance.hotbarSize)
        {
            HotbarUIController.instance.weapons[numWeapons] = newWeapon.GetComponent<WeaponItemController>();

            // Instantiate the weapon as a child of the player’s rigidbody transform.
            weapon = Instantiate(newWeapon, rb.transform);
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
                    StartCoroutine(WaitFor(0.1f));
                    if (weapon.inUse)
                        return;
                }
                if (i == index)
                    weapon.SetSelected();
                else
                    weapon.SetUnselected();
            }
        }
    }

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
            health = Mathf.Min(maxHealth, health + healAmount);
            HealthUIController.UpdateHearts(health, maxHealth);
        }
    }

    /// <summary>
    /// Gradually heals the player over time.
    /// </summary>
    /// <param name="timePerUnit">Time per healing unit (sec)</param>
    /// <param name="healAmount">Total healing amount</param>
    protected override IEnumerator GradualHeal(float timePerUnit, float healAmount)
    {
        health = Mathf.Min(maxHealth, health + 1);
        HealthUIController.UpdateHearts(health, maxHealth);
        for (int i = 0; i < healAmount - 1; i++)
        {
            yield return new WaitForSeconds(timePerUnit);
            health = Mathf.Min(maxHealth, health + 1);
            HealthUIController.UpdateHearts(health, maxHealth);
        }
    }

    protected override void UpdateMana(float mana)
    {
        ManaUIController.UpdateManaUI(mana);
    }

    protected override void UpdateHealth(float health, float maxHealth)
    {
        HealthUIController.UpdateHearts(health, maxHealth);
    }

    private IEnumerator WaitFor(float sec)
    {
        yield return new WaitForSeconds(sec);
    }
}
