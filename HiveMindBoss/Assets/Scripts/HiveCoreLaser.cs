using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiveCoreLaser : MonoBehaviour
{
    public enum eHiveCoreLaserStates
    {
        idle,
        charge,
        extend,
        fire,
        retract,
    }
    float stateTimer = 0f;

    [Header("Inscribed")]
    public int damage = 25;
    public float aimSpeed = 0.1f;
    public float idleTime = 1f;
    public float chargeTime = 1f;
    public float extendTime = 0.5f;
    public float fireTime = 5f;
    public float retractTime = 1.5f;
    public float maxZScale = 60f;

    [Header("Dynamic")]
    [SerializeField]
    eHiveCoreLaserStates state = eHiveCoreLaserStates.charge;


    Transform playerTrans;
    float minZScale;
    float zScale;

    private void Start()
    {
        playerTrans = GameObject.Find("Player").transform;
        if (playerTrans == null)
            Debug.LogError("HiveCoreLaser:Start() - playerTrans is null.");

        zScale = transform.localScale.z;
        minZScale = zScale;
    }

    private void Update()
    {
        switch (state)
        {
            case eHiveCoreLaserStates.idle:
                transform.LookAt(playerTrans.position);
                if (stateTimer >= idleTime)
                    SetState(eHiveCoreLaserStates.charge);
                break;
            case eHiveCoreLaserStates.charge:
                if (stateTimer >= chargeTime)
                    SetState(eHiveCoreLaserStates.extend);
                break;
            case eHiveCoreLaserStates.extend:
                transform.localScale = new Vector3(
                    transform.localScale.x,
                    transform.localScale.y,
                    Mathf.Lerp(zScale, maxZScale, stateTimer / extendTime));
                if (stateTimer >= extendTime)
                    SetState(eHiveCoreLaserStates.fire);
                break;
            case eHiveCoreLaserStates.fire:
                Vector3 dir = playerTrans.position - Hive.CorePosition;
                Quaternion rot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, rot, aimSpeed * Time.deltaTime);
                if (stateTimer >= fireTime)
                    SetState(eHiveCoreLaserStates.retract);
                break;
            case eHiveCoreLaserStates.retract:
                transform.localScale = new Vector3(
                    transform.localScale.x,
                    transform.localScale.y,
                    Mathf.Lerp(maxZScale, minZScale, stateTimer / retractTime));
                if (stateTimer >= retractTime)
                    SetState(eHiveCoreLaserStates.idle);
                break;
            default:
                break;
        }

        // Update position.
        transform.position = Hive.CorePosition + transform.forward * (zScale * 0.5f);

        stateTimer += Time.deltaTime;
    }

    void SetState(eHiveCoreLaserStates _newState)
    {
        stateTimer = 0f;

        switch (_newState)
        {
            case eHiveCoreLaserStates.idle:
                transform.localScale = new Vector3(
                    transform.localScale.x,
                    transform.localScale.y,
                    minZScale);
                break;
            case eHiveCoreLaserStates.charge:
                break;
            case eHiveCoreLaserStates.extend:
                break;
            case eHiveCoreLaserStates.fire:
                break;
            case eHiveCoreLaserStates.retract:
                transform.localScale = new Vector3(
                    transform.localScale.x,
                    transform.localScale.y,
                    maxZScale);
                break;
            default:
                break;
        }

        state = _newState;
    }

    /*
    public void OnTriggerEnter(Collision collision)
    {
        Collider myCollider = collision.contacts[0].thisCollider;

        if (myCollider.gameObject.tag == "Player")
        {
            myCollider.gameObject.GetComponent<PlayerHealth>().TakeDamage(damage);
        }
    }
    */

    public void Fire()
    {
        transform.LookAt(playerTrans.position);

        SetState(eHiveCoreLaserStates.charge);
    }

    public int Damage
    {
        get { return damage; }
    }
}
