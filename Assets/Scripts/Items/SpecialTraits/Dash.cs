using System.Collections;
using UnityEngine;

public abstract class Dash : SpecialTrait
{
    /// <summary>
    /// Whether we are cooling down or can dash
    /// </summary>
    public bool coolingDown = false;
    /// <summary>
    /// How long until dash is available again
    /// </summary>
    public float cooldownTime = 3;
    /// <summary>
    /// How much force is used per push on player
    /// </summary>
    public float force = 1f;
    /// <summary>
    /// How long the dash is active
    /// </summary>
    public float duration = 0.2f;
    /// <summary>
    /// How often the force is activated in the dash (in sec)
    /// </summary>
    public float forceFrequency = 0.01f;
    /// <summary>
    /// How much time is left in the dash
    /// </summary>
    protected float timeLeftDashing;
    /// <summary>
    /// How long after the first tap can the second be tapped for a dash to activate
    /// </summary>
    protected float doubleClickWindow = 0.25f;
    /// <summary>
    /// Which key was last pressed
    /// </summary>
    private KeyCode lastKeyPressed;
    /// <summary>
    /// When the last key was pressed
    /// </summary>
    private float lastKeyPressTime;
    /// <summary>
    /// Whether a tap is the first one
    /// </summary>
    private bool isFirstTap = true;

    void Update()
    {
        if (coolingDown || user == null)
            return;

        if (user.GetComponent<Player>() != null)
        {
            // Check each direction input
            StartCoroutine(CheckForPlayerDash(KeyCode.W, Vector3.forward));
            StartCoroutine(CheckForPlayerDash(KeyCode.A, Vector3.left));
            StartCoroutine(CheckForPlayerDash(KeyCode.S, Vector3.back));
            StartCoroutine(CheckForPlayerDash(KeyCode.D, Vector3.right));
        }
        else
        {
            StartCoroutine(CheckForEnemyDash());
        }
    }

    /// <summary>Check whether key has been double tapped to dash</summary>
    /// <param name="key">Which key is being checked</param>
    /// <param name="direction">Direction to dash in</param>
    private IEnumerator CheckForPlayerDash(KeyCode key, Vector3 direction)
    {
        if (Input.GetKeyDown(key))
        {
            // First tap detection
            if (isFirstTap || (lastKeyPressed == key && Time.time - lastKeyPressTime <= doubleClickWindow))
            {
                if (!isFirstTap) // If it's the second tap within the window, dash
                {
                    timeLeftDashing = duration;
                    while (timeLeftDashing > 0)
                    {
                        DoDash(direction);
                        timeLeftDashing -= forceFrequency;
                        yield return new WaitForSeconds(forceFrequency);
                    }
                    StartCoroutine(Cooldown());
                    isFirstTap = true; // Reset for the next dash
                }
                else // If it's the first tap, store the key and time
                {
                    lastKeyPressed = key;
                    lastKeyPressTime = Time.time;
                    isFirstTap = false;
                }
            }
            else // Reset if a different key is pressed
            {
                lastKeyPressed = key;
                lastKeyPressTime = Time.time;
            }
        }

        // Reset if the double-click window passes
        if (Time.time - lastKeyPressTime > doubleClickWindow)
        {
            isFirstTap = true;
        }
    }

    /// <summary>Check whether focus is far enough to want to dash</summary>
    protected IEnumerator CheckForEnemyDash()
    {
        Enemy c = gameObject.GetComponent<Enemy>();
        if (c != null && c.focus != null && !coolingDown)
        {
            if ((c.currState == CharacterState.follow || c.currState == CharacterState.flee) && Vector3.Distance(c.targetPos, transform.position) > 5)
            {
                Vector3 direction = (c.targetPos - transform.position).normalized;
                timeLeftDashing = duration;
                while (timeLeftDashing > 0)
                {
                    DoDash(direction);
                    timeLeftDashing -= forceFrequency;
                    yield return new WaitForSeconds(forceFrequency);
                }
                StartCoroutine(Cooldown());
            }
        }
    }

    /// <summary>
    /// Do the dash in the direction
    /// </summary>
    /// <param name="direction">Direction to dash in</param>
    protected abstract void DoDash(Vector3 direction);

    /// <summary>
    /// Start a cooldown until it can dash again
    /// </summary>
    protected IEnumerator Cooldown()
    {
        coolingDown = true;
        yield return new WaitForSeconds(cooldownTime);
        coolingDown = false;
    }
}
