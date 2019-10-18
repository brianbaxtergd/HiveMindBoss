using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomRotation : MonoBehaviour
{
    [Header("Set in Inspector")]
    [Tooltip("Minimum rotation speed for x, y, and z axis.")]
    public float minSpeed;
    [Tooltip("Maximum rotation speed for x, y, and z axis.")]
    public float maxSpeed;

    float xSpeed;
    float ySpeed;
    float zSpeed;

    void Start()
    {
        // Randomize x, y, and z axis rotation speeds.
        xSpeed = Random.Range(minSpeed, maxSpeed);
        ySpeed = Random.Range(minSpeed, maxSpeed);
        zSpeed = Random.Range(minSpeed, maxSpeed);
    }

    void FixedUpdate()
    {
        // Rotate the transform.
        transform.Rotate(xSpeed, ySpeed, zSpeed);
    }
}
