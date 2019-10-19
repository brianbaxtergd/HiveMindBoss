using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiveController : MonoBehaviour
{
    public enum eHiveStates
    {
        spawningDrones,
        rotate,
    }

    [Header("Inscribed")]
    public GameObject dronePrefab;
    [Range(10, 250)]
    public int droneCount;
    [Range(5f, 25f)]
    public float droneRadius;
    public float droneTimeBetweenSpawns;
    [Tooltip("Measured in degrees per second.")]
    public Vector3 hiveRotationSpeed;

    [Header("Dynamic")]
    [SerializeField]
    List<GameObject> drones;
    [SerializeField]
    List<DroneController> droneControllers;
    [SerializeField]
    List<Vector3> dronePositions;
    [SerializeField]
    eHiveStates hiveState;

    GameObject cube;
    GameObject sphere;
    GameObject droneAnchor;

    private void Start()
    {
        // Find references to existing child objects.
        cube = GameObject.Find("HiveCube");
        if (cube == null)
            Debug.LogError("HiveController:Start - HiveCube could not be found.");
        sphere = GameObject.Find("HiveSphere");
        if (sphere == null)
            Debug.LogError("HiveController:Start - HiveSphere could not be found.");

        // Instantiate an empty GameObject to hold drones in hierarchy.
        droneAnchor = new GameObject("DroneAnchor");
        droneAnchor.transform.SetParent(transform);

        drones = new List<GameObject>();
        dronePositions = new List<Vector3>();

        InitializeDronePositions(droneCount, droneRadius, false);

        StartCoroutine(SpawnDrones(droneTimeBetweenSpawns));

        SetState(eHiveStates.rotate);
    }

    private void Update()
    {
        //DebugDrawDronePositions();
    }

    private void FixedUpdate()
    {
        switch (hiveState)
        {
            case eHiveStates.rotate:
                RotateDronePositions(hiveRotationSpeed * Time.fixedDeltaTime);
                UpdateDronePositions();
                break;
            default:
                break;
        }
    }

    void SetState(eHiveStates _newState)
    {
        hiveState = _newState;
    }

    void InitializeDronePositions()
    {
        float angle = 0f;
        float angleStep = 360f / droneCount;
        float radius = 6f;
        for (int i = 0; i < droneCount; i++)
        {
            // Find new position.
            Vector3 newPos = transform.position + Quaternion.AngleAxis(angle, transform.forward) * transform.up * radius;
            // Store position.
            dronePositions.Add(newPos);
            // Update fields.
            angle += angleStep;
        }
    }

    void InitializeDronePositions(int _samples, float _radius, bool _randomize)
    {
        int rand = 1;
        if (_randomize)
            rand = Random.Range(rand, _samples);

        Vector3[] points = new Vector3[_samples];
        float offset = 2f / _samples;
        float increment = Mathf.PI * (3f - Mathf.Sqrt(5f));
        for (int i = 0; i < _samples; i++)
        {
            float y = ((i * offset) - 1) + (offset / 2f);
            float r = Mathf.Sqrt(1 - Mathf.Pow(y, 2));

            float phi = ((i + rand) % _samples) * increment;

            float x = Mathf.Cos(phi) * r;
            float z = Mathf.Sin(phi) * r;

            dronePositions.Add(new Vector3(x, y, z) * _radius);
        }
    }

    void UpdateDronePositions()
    {
        for (int i = 0; i < droneControllers.Count; i++)
        {
            droneControllers[i].TargetPosition = dronePositions[i];
        }
    }

    void RotateDronePositions(Vector3 _angle)
    {
        Vector3 dir;
        for (int i = 0; i < dronePositions.Count; i++)
        {
            dir = dronePositions[i] - transform.position;
            dir = Quaternion.Euler(_angle) * dir;
            dronePositions[i] = dir + transform.position;
        }
    }

    /// <summary>
    /// Instantiates an instance of the dronePrefab GameObject, then stores both stores its reference within the drones list and its DroneController component with the droneControllers list.
    /// </summary>
    /// <param name="_index">Index within drones list.</param>
    /// <param name="_targetPosition">The initial target position for the Drone to move towards.</param>
    void AddDrone(int _index, Vector3 _targetPosition)
    {
        // Initialize instance of drone prefab.
        GameObject drone = Instantiate(dronePrefab);
        drone.transform.SetParent(droneAnchor.transform);
        DroneController dC = drone.GetComponent<DroneController>();
        dC.Index = _index;
        dC.TargetPosition = _targetPosition;
        // Store reference to instance.
        drones.Add(drone);
        // Store reference to instance's DroneController component.
        droneControllers.Add(dC);
    }

    IEnumerator SpawnDrones(float _timeBetweenSpawns)
    {
        for (int i = 0; i < droneCount; i++)
        {
            AddDrone(i, dronePositions[i]);
            yield return new WaitForSeconds(_timeBetweenSpawns);
        }
    }

    /// Debug Methods ///

    void DebugDrawDronePositions()
    {
        for (int i = 0; i < dronePositions.Count; i++)
        {
            Debug.DrawLine(transform.position, dronePositions[i], Color.white);
        }
    }
}
