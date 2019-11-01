using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(ShimmerColor))]
public class Drone : MonoBehaviour
{
    int index = 0; // Index position within Hive's activeDrones list.
    int health;
    bool isAlive = true;
    bool isAttacking = false;
    public bool recalledByHive = false;
    Vector3 targetPosition;
    Renderer rend;
    ShimmerColor shimCol;
    //Color defaultColor;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        shimCol = GetComponent<ShimmerColor>();

        //defaultColor = Hive.DronesSO.idleMaterial.GetColor("_EmissionColor"); //rend.material.GetColor("_EmissionColor");
    }

    private void FixedUpdate()
    {
        UpdateTargetPosition();
        UpdatePosition();

        if (isAlive)
        {
            if (isAttacking && Vector3.Distance(transform.position, targetPosition) < 0.01f)
                IsAttacking = false;
        }
        else
        {
            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
                Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        Hive.RemoveDrone(index);
    }

    void UpdateTargetPosition()
    {
        if (isAlive)
        {
            if (!isAttacking)
                targetPosition = Hive.DronePositions[index];
        }
        else
        {
            targetPosition = Hive.CorePosition;
        }
    }

    void UpdatePosition()
    {
        // Move toward target position.
        float step = Hive.DronesSO.moveSpeed * Time.fixedDeltaTime;
        transform.position = Vector3.MoveTowards(transform.position, TargetPosition, step);
    }

    IEnumerator ChangeMaterialColor(Material _curMat, Material _newMat)
    {
        //defaultColor = _newMat.GetColor("_EmissionColor");
        shimCol.DefaultColor = _newMat.GetColor("_EmissionColor");
        float elapsed = 0f;
        yield return null;

        while(elapsed < Hive.DronesSO.colorChangeTime)
        {
            rend.material.Lerp(_curMat, _newMat, elapsed / Hive.DronesSO.colorChangeTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rend.material = _newMat;
    }

    public void TakeDamage(int _damage)
    {
        if (!isAlive)
            return;

        health = Mathf.Max(health - _damage, 0);
        if (health == 0)
            Death();
    }

    public void Death()
    {
        isAlive = false;
        health = 0;
        StartCoroutine(ChangeMaterialColor(rend.material, Hive.DronesSO.deadMaterial));
    }

    public void Attack(Vector3 _targetPosition)
    {
        if (isAttacking)
        {
            Debug.Log("DronController:Attack() - Drone sent to attack while already attacking.");
            return;
        }

        isAttacking = true;
        TargetPosition = _targetPosition;
        StartCoroutine(ChangeMaterialColor(Hive.DronesSO.idleMaterial, Hive.DronesSO.attackMaterial));
    }

    public void Shimmer()
    {
        shimCol.Shimmer(Hive.DronesSO.shimmerColor, Hive.DronesSO.shimmerTime);
    }

    public void Shimmer(Color _color, float _shimmerTime)
    {
        shimCol.Shimmer(_color, _shimmerTime);
    }

    // Properties.

    public int Health
    {
        get { return health; }
        set { health = value; }
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
    public bool IsAlive
    {
        get { return isAlive; }
        private set { isAlive = value; }
    }
    public bool IsAttacking
    {
        get { return isAttacking; }
        set
        {
            isAttacking = value;
            if (!isAttacking)
                StartCoroutine(ChangeMaterialColor(Hive.DronesSO.attackMaterial, Hive.DronesSO.idleMaterial));
        }
    }
    /*
    public Color DefaultColor
    {
        get { return defaultColor; }
        set { defaultColor = value; }
    }
    */

    // Statics.

    static public Drone SpawnDrone()
    {
        GameObject dGO = Instantiate<GameObject>(Hive.DronesSO.dronePrefab);
        Drone drone = dGO.GetComponent<Drone>();
        return drone;
    }
}
