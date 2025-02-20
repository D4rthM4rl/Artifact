using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicSword : Weapon
{
    //Getting Rigid body, defining mousePos for use in angling the weapon at the mouse cursor
    SpriteRenderer spi;
    public Rigidbody2D rb;
    public Vector2 mousePos;
    private Vector3 initialRotation;

    private float canSwing = 1;
    // public Transform firepoint;
    private Transform holdPoint;

    public float swingRate;
    public float swingSpeed = 20f;
    public float swordSize;

    public float angle;
    Rigidbody2D playerRb;

    // Start is called before the first frame update
    void Start()
    {
        spi = GetComponent<SpriteRenderer>();
        holdPoint = GameObject.Find("HoldPivot").transform;
        playerRb = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody2D>();
        initialRotation = transform.eulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 objectSize = GetComponent<SpriteRenderer>().bounds.size; // Get the size of the object
        Vector3 newPosition = transform.position; // Get the current position of the object

        // Adjust the position based on half of the object's size
        newPosition.x = (objectSize.x / 2f) + holdPoint.transform.position.x; // Adjust along the X-axis by half of the object's width
        newPosition.y = (objectSize.y / 2f) + holdPoint.transform.position.y - 0.2f; // Adjust along the Y-axis by half of the object's height

        if (Input.GetButton("Fire1") && Time.time > canSwing && isSelected)
        {
            Debug.Log("swinging");
            StartCoroutine(Attack());
            canSwing = Time.time + swingRate;
        }
        //Setting mousePos to the mouse location as a vector 2, and subtracting the rigid body position,
        //vector math, taking arctan of line to determine angle in radians, correcting for error with -180
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 lookDir = mousePos - playerRb.position;
        angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 180;
        // rb.rotation = angle;

        if (angle >= -90 - 52.5 && angle <= -52)
        {
            spi.sortingOrder = -5;
        }
        else 
        {
            spi.sortingOrder = 5;
        }

        //flipping sprite over y axis when cursor is on left half of screen to maintain proper orientation
        if (angle <= -90 && angle >= -270)
        {
            spi.flipX = false;
            newPosition.x = (objectSize.x / 2f) + holdPoint.transform.position.x;
        }
        else
        {
            spi.flipX = true;
            newPosition.x = -(objectSize.x / 2f) + holdPoint.transform.position.x;
        }

        // Set the new position
        transform.position = newPosition;
    }

    IEnumerator Attack()
    {
        inUse = true;
        float elapsedTime = 0f;
        float swingDuration = 0.3f; // Adjust this value for the desired swing duration

        Vector3 pivotPoint = holdPoint.position; // Set pivot point to the hold point position

        // Get the distance from the pivot point to the center of the sword
        Vector3 offset = transform.position - pivotPoint;

        while (elapsedTime < swingDuration)
        {
            // Rotate the sword around the pivot point
            transform.RotateAround(pivotPoint, Vector3.forward, 360f * Time.deltaTime / swingDuration);

            // Maintain the offset to keep the hilt in place
            transform.position = pivotPoint + offset;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the sword returns to its initial rotation
        transform.rotation = Quaternion.identity;
        inUse = false;
    }
}