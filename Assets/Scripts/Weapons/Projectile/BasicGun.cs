using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicGun : ProjectileWeapon
{
    // private void Start() {
    //     PWStart();
    // }

    // private void FixedUpdate() {
    //     PWFixedUpdate();
    // }

    // private void Update() {
    //     PWUpdate();
    // }
    // void FixedUpdate() 
    // {
    //     //Setting mousePos to the mouse location as a vector 2, and subtracting the rigid body position,
    //     //vector math, taking arctan of line to determine angle in radians, correcting for error with -180
    //     mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    //     Vector2 lookDir = mousePos - rb.position;
    //     float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 180;
    //     rb.rotation = angle;

    //     if (angle >= -90 - 52.5 && angle <= -52)
    //     {
    //         spi.sortingOrder = -5;
    //     }
    //     else 
    //     {
    //         spi.sortingOrder = 5;
    //     }

    //     //flipping sprite over y axis when cursor is on left half of screen to maintain proper orientation
    //     if (angle <= -90 && angle >= -270)
    //     {
    //         spi.flipY = true;
    //     }
    //     else
    //     {
    //         spi.flipY = false;
    //     }
    // }
}   
