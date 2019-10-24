﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiveLaser : MonoBehaviour
{
    public enum eHiveLaserStates
    {
        idle,
        charge,
        extend,
        fire,
        retract,
    }
    float stateTimer = 0f;

    [Header("Inscribed")]
    public int damage;
    public float rotationSpeed;
    public float chargeTime;
    public float extendTime;
    public float fireTime;
    public float retractTime;
    public Vector3 minScale;
    public float maxZScale;

    [Header("Dynamic")]
    [SerializeField]
    eHiveLaserStates state;

    Transform playerTrans;
    float minZScale;
    float zScale;
    Vector3 idleScale;

    private void Start()
    {
        playerTrans = GameObject.Find("Player").transform;
        if (playerTrans == null)
            Debug.LogError("HiveLaser:Start() - playerTrans is null.");

        zScale = minScale.z;
        minZScale = zScale;
        idleScale = Vector3.zero;

        SetState(eHiveLaserStates.idle);
    }

    private void Update()
    {
        switch (state)
        {
            case eHiveLaserStates.idle:
                if (Input.GetKeyDown(KeyCode.Space))
                    Fire();
                break;
            case eHiveLaserStates.charge:
                if (stateTimer >= chargeTime)
                    SetState(eHiveLaserStates.extend);
                break;
            case eHiveLaserStates.extend:
                // Rotate towards player.
                RotateTowardsTarget(playerTrans.position);
                // Update scale.
                transform.localScale = new Vector3(
                    transform.localScale.x,
                    transform.localScale.y,
                    Mathf.Lerp(zScale, maxZScale, stateTimer / extendTime));
                if (stateTimer >= extendTime)
                    SetState(eHiveLaserStates.fire);
                break;
            case eHiveLaserStates.fire:
                // Rotate towards player.
                RotateTowardsTarget(playerTrans.position);
                if (stateTimer >= fireTime)
                    SetState(eHiveLaserStates.retract);
                break;
            case eHiveLaserStates.retract:
                // Update scale.
                transform.localScale = new Vector3(
                    Mathf.Lerp(minScale.x, idleScale.x, stateTimer / retractTime),
                    Mathf.Lerp(minScale.y, idleScale.y, stateTimer / retractTime),
                    maxZScale);
                if (stateTimer >= retractTime)
                    SetState(eHiveLaserStates.idle);
                break;
            default:
                break;
        }

        // Update position.
        //transform.localPosition = transform.forward * (transform.lossyScale.z - minScale.z * transform.localScale.z) * 0.5f;
        transform.localPosition = transform.forward * (minScale.z * transform.localScale.z - transform.lossyScale.z) * 0.5f;


        // Update state timer.
        stateTimer += Time.deltaTime;
    }

    void SetState(eHiveLaserStates _newState)
    {
        stateTimer = 0f;

        switch (_newState)
        {
            case eHiveLaserStates.idle:
                transform.localScale = idleScale;
                break;
            case eHiveLaserStates.charge:
                transform.localScale = minScale;
                break;
            case eHiveLaserStates.extend:
                break;
            case eHiveLaserStates.fire:
                break;
            case eHiveLaserStates.retract:
                transform.localScale = new Vector3(
                    minScale.x,
                    minScale.y,
                    maxZScale);
                break;
            default:
                break;
        }

        state = _newState;
    }

    void RotateTowardsTarget(Vector3 _targetPosition)
    {
        Vector3 targetDirection = _targetPosition - Hive.CorePosition;
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    public void Fire()
    {
        // Aim at player's current position.
        transform.LookAt(playerTrans.position);
        // Switch to charge state.
        SetState(eHiveLaserStates.charge);
    }

    public void StopFiring()
    {
        // Switch to retract state.
        SetState(eHiveLaserStates.retract);
    }

    public int Damage
    {
        get { return damage; }
    }
}