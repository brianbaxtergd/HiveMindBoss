using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DroneMovement : MonoBehaviour
{
    [Header("Set in Inspector")]
    public float maxSpeed;
    [Tooltip("The amount of time in seconds required to accelerate to full speed.")]
    public float accelTime;

    float speed;
    Vector3 targetPosition;
    Rigidbody rigid;

    void Start()
    {
        speed = 0f;
        rigid = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // Update speed via acceleration.
        if (speed < maxSpeed)
            speed = Mathf.Min(speed + maxSpeed * Time.deltaTime / accelTime, maxSpeed);
        // Determine the vector towards target.
        Vector3 direction = targetPosition - transform.position;
        direction.Normalize();
        // Update velocity.
        float distance = Vector3.Distance(transform.position, targetPosition);
        if (distance > speed * Time.deltaTime)
        {
            rigid.velocity = direction * speed;
        }
        else
        {
            rigid.velocity = direction * distance * 0.25f;
        }
    }

    public Vector3 TargetPosition
    {
        get
        {
            return targetPosition;
        }
        set
        {
            targetPosition = value;
        }
    }
}
