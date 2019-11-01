using System.Collections;
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
    Transform coreTrans;
    float minZScale;
    float zScale;
    Vector3 idleScale;
    Vector3 chargeScale;

    private void Awake()
    {
        coreTrans = GameObject.Find("HiveCore").transform;
        if (coreTrans == null)
            Debug.LogError("HiveLaser:Awake() - coreTrans is null.");
        playerTrans = GameObject.Find("Player").transform;
        if (playerTrans == null)
            Debug.LogError("HiveLaser:Awake() - playerTrans is null.");
    }

    private void Start()
    {
        zScale = minScale.z;
        minZScale = zScale;
        idleScale = Vector3.zero;
        chargeScale = minScale;
        chargeScale.z = 2f;

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
                transform.localScale = Vector3.Lerp(idleScale, chargeScale, stateTimer / (chargeTime * 0.34f));
                if (stateTimer >= chargeTime)
                    SetState(eHiveLaserStates.extend);
                break;
            case eHiveLaserStates.extend:
                // Rotate towards player.
                RotateTowardsTarget(playerTrans.position);
                // Update scale.
                transform.localScale = new Vector3(
                    minScale.x,
                    minScale.y,
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
        transform.localPosition = (transform.forward * (coreTrans.localScale.x * 0.85f)) + transform.forward * (minScale.z * transform.localScale.z - transform.lossyScale.z) * 0.5f; // Not entirely sure why this works as intended.. although I've noted it only works when minScale.z = 3;

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
                break;
            case eHiveLaserStates.extend:
                transform.localScale = minScale;
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
    public bool IsFiring
    {
        get { return (state != eHiveLaserStates.idle && state != eHiveLaserStates.retract); }
    }
}
