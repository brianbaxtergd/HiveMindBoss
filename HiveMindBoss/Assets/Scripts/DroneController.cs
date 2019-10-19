using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneController : MonoBehaviour
{
    [Header("Inscribed")]
    public float moveSpeed = 1f;

    int index = 0; // Index position within HiveController's drones list.
    Vector3 targetPosition;

    void FixedUpdate()
    {
        UpdatePosition();
    }

    void UpdatePosition()
    {
        float step = moveSpeed * Time.fixedDeltaTime;
        transform.position = Vector3.MoveTowards(transform.position, TargetPosition, step);
    }

    public int Index
    {
        get { return index; }
        set { index = value; }
    }

    public Vector3 TargetPosition
    {
        get { return targetPosition; }
        set { targetPosition = value; }
    }
}
