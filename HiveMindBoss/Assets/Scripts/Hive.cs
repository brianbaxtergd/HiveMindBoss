﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hive : MonoBehaviour
{
    static private Hive _S; // Unprotected singleton.

    public enum eHiveStates
    {
        spawningDrones,
        idle,
        attack,
    }

    public enum eHiveTopologies
    {
        sphere,
        layeredSphere,
        doubleRing,
    }

    [Header("Inscribed - Drones")]
    public GameObject dronePrefab;
    [Range(10, 250)]
    public int droneCount;
    [Range(5, 25)]
    public float droneDistance; // From center of Hive.
    public float droneTimeBetweenSpawns;

    [Header("Inscribed - Hive")]
    public Vector3 hiveRotationIdle;
    public Vector3 hiveRotationAttacking;
    public float idleStateTime = 10f;
    public int attackStateDroneCount = 5;

    [Header("Dynamic")]
    [SerializeField]
    eHiveStates hiveState;
    [SerializeField]
    eHiveTopologies hiveTopology;
    [SerializeField]
    List<Drone> activeDrones;
    [SerializeField]
    List<Vector3> dronePositions;

    GameObject player;
    GameObject core;
    GameObject shield;
    HiveLaser laser;
    GameObject droneAnchor;

    float hiveStateTime = 0f;

    private void Start()
    {
        if (_S == null)
            _S = this;

        // Initialize data structures.
        activeDrones = new List<Drone>();
        dronePositions = new List<Vector3>();

        // Find and store references.
        player = GameObject.Find("Player");
        if (player == null)
            Debug.LogError("Hive:Start() - player is null.");
        core = GameObject.Find("HiveCore");
        if (core == null)
            Debug.LogError("Hive:Start() - core is null.");
        shield = GameObject.Find("HiveCoreShield");
        if (shield == null)
            Debug.LogError("Hive:Start() - shield is null.");
        laser = GameObject.Find("HiveLaser").GetComponent<HiveLaser>();
        if (laser == null)
            Debug.LogError("Hive:Start() - laser is null");

        // Spawn empty game object to act as drone anchor in hierarchy.
        droneAnchor = new GameObject("DroneAnchor");
        droneAnchor.transform.SetParent(transform);

        // Initialize dronePositions and instantiate drones.
        DefineDronePositions(eHiveTopologies.sphere);
        StartCoroutine(SpawnDrones());
    }

    private void FixedUpdate()
    {
        switch (hiveState)
        {
            case eHiveStates.spawningDrones:
                break;
            case eHiveStates.idle:
                RotateDronePositions(hiveRotationIdle * Time.fixedDeltaTime);
                if (hiveStateTime >= idleStateTime)
                    SetState(eHiveStates.attack);
                break;
            case eHiveStates.attack:
                RotateDronePositions(hiveRotationAttacking * Time.fixedDeltaTime);
                break;
            default:
                break;
        }

        // Update state timer.
        hiveStateTime += Time.fixedDeltaTime;
    }

    void SetState(eHiveStates _newState)
    {
        // Reset state timer.
        hiveStateTime = 0f;

        // Initialize new state.
        switch (_newState)
        {
            case eHiveStates.spawningDrones:
                break;
            case eHiveStates.idle:
                // Trigger shimmer effect on all drones.
                StartCoroutine(DroneShimmer());
                break;
            case eHiveStates.attack:
                // Begin sending drones to attack player.
                StartCoroutine(DroneAttack(attackStateDroneCount));
                break;
            default:
                break;
        }
        hiveState = _newState;
    }

    void DefineDronePositions(eHiveTopologies _hiveTopology)
    {
        // Clear list of existing positions.
        if (dronePositions.Count > 0)
            dronePositions.Clear();

        switch (_hiveTopology)
        {
            case eHiveTopologies.sphere:
                // Define and store points around a sphere based on current droneCount and droneDistance.
                float offset = 2f / droneCount;
                float increment = Mathf.PI * (3f - Mathf.Sqrt(5f));
                for (int i = 0; i < droneCount; i++)
                {
                    float y = ((i * offset) - 1) + (offset / 2f);
                    float r = Mathf.Sqrt(1 - Mathf.Pow(y, 2));
                    float phi = (i % droneCount) * increment;
                    float x = Mathf.Cos(phi) * r;
                    float z = Mathf.Sin(phi) * r;
                    dronePositions.Add(new Vector3(x, y, z) * droneDistance);
                }
                break;
            case eHiveTopologies.layeredSphere:
                break;
            case eHiveTopologies.doubleRing:
                int totalDrones = droneCount;
                int innerRingDroneCount, outerRingDroneCount;
                outerRingDroneCount = totalDrones / 2;
                totalDrones -= totalDrones / 2;
                innerRingDroneCount = totalDrones;
                float angleIncrement, radius;
                Vector3 firstPosition;

                angleIncrement = 360f / outerRingDroneCount;
                radius = droneDistance;
                firstPosition = core.transform.position + new Vector3(0, radius, 0);
                for (int i = 0; i < outerRingDroneCount; i++)
                {
                    Vector3 dir = firstPosition - core.transform.position;
                    dir = Quaternion.Euler(new Vector3(0, 0, i * angleIncrement)) * dir;
                    dronePositions.Add(dir + core.transform.position);
                }

                angleIncrement = 360f / innerRingDroneCount;
                radius = droneDistance * 0.5f;
                firstPosition = core.transform.position + new Vector3(0, radius, 0);
                for (int i = 0; i < innerRingDroneCount; i++)
                {
                    Vector3 dir = firstPosition - core.transform.position;
                    dir = Quaternion.Euler(new Vector3(0, 0, i * angleIncrement)) * dir;
                    dronePositions.Add(dir + core.transform.position);
                }
                break;
            default:
                break;
        }
        hiveTopology = _hiveTopology;
    }

    void RotateDronePositions(Vector3 _rotation)
    {
        Vector3 dir;
        for (int i = 0; i < dronePositions.Count; i++)
        {
            dir = dronePositions[i] - core.transform.position;
            dir = Quaternion.Euler(_rotation) * dir;
            dronePositions[i] = dir + core.transform.position;
        }
    }

    Drone GetDroneNearestToPlayer()
    {
        float nearestDist = 1000f;
        int nearestDroneIndex = 0;
        for (int i = 0; i < activeDrones.Count; i++)
        {
            // If this drone has died or is attacking, skip this drone.
            if (!activeDrones[i].IsAlive 
                || activeDrones[i].IsAttacking 
                || Vector3.Distance(activeDrones[i].transform.position, activeDrones[i].TargetPosition) > 1f) // If drone is returning from attack, skip this drone.
                continue;

            float dist = Vector3.Distance(activeDrones[i].transform.position, player.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearestDroneIndex = i;
            }
        }

        return activeDrones[nearestDroneIndex];
    }

    IEnumerator SpawnDrones()
    {
        SetState(eHiveStates.spawningDrones);

        if (activeDrones.Count > 0)
            Debug.LogError("Hive:SpawnDrones() - activeDrones not cleared before spawning new drones.");

        for (int i = 0; i < droneCount; i++)
        {
            GameObject droneGO = Instantiate(dronePrefab);
            droneGO.transform.SetParent(droneAnchor.transform);
            Drone drone = droneGO.GetComponent<Drone>();
            drone.Index = i;
            drone.TargetPosition = dronePositions[i];
            activeDrones.Add(drone);

            yield return new WaitForSeconds(droneTimeBetweenSpawns);
        }

        SetState(eHiveStates.idle);
    }

    IEnumerator DroneAttack(int _droneCount)
    {
        _droneCount = Mathf.Clamp(_droneCount, 0, activeDrones.Count);

        for (int i = 0; i < _droneCount; i++)
        {
            Drone drone = GetDroneNearestToPlayer();
            if (drone != null)
                drone.Attack(player.transform.position);
            yield return new WaitForSeconds(1f);
        }

        SetState(eHiveStates.idle);
    }

    IEnumerator DroneShimmer()
    {
        for (int i = 0; i < activeDrones.Count; i++)
        {
            activeDrones[i].Shimmer();
            yield return null;
        }
    }

    static public List<Vector3> DronePositions
    {
        get { return _S.dronePositions; }
    }

    static public void RemoveDrone(int _index)
    {
        // Trigger core shimmer.
        _S.core.GetComponent<ShimmerColor>().Shimmer(_S.activeDrones[_index].DefaultColor);
        // Remove drone.
        _S.activeDrones.RemoveAt(_index);
        // Remove position of drone.
        _S.dronePositions.RemoveAt(_index);
        // Re-assign drone indices.
        for (int i = 0; i < _S.activeDrones.Count; i++)
        {
            _S.activeDrones[i].Index = i;
        }
    }

    static public Vector3 CorePosition
    {
        get { return _S.core.transform.position; }
    }
}
