﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Inscribed")]
    public float speedMax;
    public float tiltSpeed;
    public float tiltMax;
    public float distanceFromHive;
    public float yMin;
    public float yMax;

    Transform hive;
    Transform guns;
    Transform crosshair;
    Quaternion gunRotationDefault;

    void Start()
    {
        hive = GameObject.Find("Hive").transform;
        if (hive == null)
            Debug.LogError("PlayerMovement:Start() - Unable to find GameObject Hive's Transform component.");

        guns = GameObject.Find("Guns").transform;
        if (guns == null)
            Debug.LogError("PlayerMovement:Start() - Unable to find GameObject Gun's Transform component.");
        gunRotationDefault = guns.localRotation;

        crosshair = GameObject.Find("Crosshair").transform;
        if (crosshair == null)
            Debug.LogError("PlayerMovement:Start() - Unable to find GameObject Crosshair's Transform component.");
    }

    void FixedUpdate()
    {
        float hor = Input.GetAxis("Horizontal");
        float ver = Input.GetAxis("Vertical");

        if (Mathf.Abs(hor) > 0f)
        {
            // Move left/right.
            transform.RotateAround(hive.position, Vector3.up, -hor * speedMax * Time.fixedDeltaTime);
            transform.LookAt(hive.position);
        }

        // Tilt with movement.
        float tiltAmount = -hor * tiltMax;
        Quaternion curRot = guns.localRotation;
        Quaternion goalRot = Quaternion.Euler(0f, 0f, tiltAmount);
        curRot = Quaternion.Slerp(curRot, goalRot, tiltSpeed * Time.fixedDeltaTime);
        guns.localRotation = curRot;
        crosshair.localRotation = curRot;

        if (Mathf.Abs(ver) > 0f)
        {
            // Move up/down.
            transform.RotateAround(hive.position, transform.right, Input.GetAxis("Vertical") * speedMax * Time.fixedDeltaTime);

            Vector3 posYClamped = transform.position;
            posYClamped.y = Mathf.Clamp(transform.position.y, yMin, yMax);
            transform.position = posYClamped;

            transform.LookAt(hive.position);

            float dist = Vector3.Distance(transform.position, hive.position);
            if (dist < distanceFromHive)
            {
                float distDelta = distanceFromHive - dist;
                transform.position += -transform.forward * distDelta;
            }
        }
    }
}
