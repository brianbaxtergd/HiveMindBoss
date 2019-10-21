using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiveController : MonoBehaviour
{
    static private HiveController _S;

    public enum eHiveStates
    {
        spawningDrones,
        rotate,
    }

    [Header("Inscribed - Drones")]
    public GameObject dronePrefab;
    [Range(10, 250)]
    public int droneCount;
    [Range(5f, 25f)]
    public float droneRadius;
    public float droneTimeBetweenSpawns;
    [Tooltip("Measured in degrees per second.")]

    [Header("Inscribed - Hive")]
    public Vector3 hiveRotationSpeed;
    public Vector3 hiveRotationSpeedAttacking;

    [Header("Dynamic")]
    [SerializeField]
    static List<GameObject> activeDrones = new List<GameObject>();
    [SerializeField]
    static List<DroneController> droneControllers = new List<DroneController>();
    [SerializeField]
    static List<Vector3> dronePositions = new List<Vector3>();
    [SerializeField]
    eHiveStates hiveState;

    GameObject cube;
    GameObject sphere;
    GameObject droneAnchor;
    GameObject player;

    private void Start()
    {
        if (_S == null)
            _S = this;

        // Find references to existing child objects.
        cube = GameObject.Find("HiveCube");
        if (cube == null)
            Debug.LogError("HiveController:Start - HiveCube could not be found.");
        sphere = GameObject.Find("HiveSphere");
        if (sphere == null)
            Debug.LogError("HiveController:Start - HiveSphere could not be found.");

        // Find reference to player.
        player = GameObject.Find("Player");
        if (player == null)
            Debug.LogError("HiveController:Start - Player could not be found.");

        // Instantiate an empty GameObject to hold drones in hierarchy.
        droneAnchor = new GameObject("DroneAnchor");
        droneAnchor.transform.SetParent(transform);

        InitializeDronePositions(droneCount, droneRadius, false);

        StartCoroutine(SpawnDrones(droneTimeBetweenSpawns));

        SetState(eHiveStates.spawningDrones);
    }

    private void Update()
    {
        //DebugDrawDronePositions();
    }

    private void FixedUpdate()
    {
        switch (hiveState)
        {
            case eHiveStates.spawningDrones:
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
        switch (_newState)
        {
            case eHiveStates.spawningDrones:
                break;
            case eHiveStates.rotate:
                StartCoroutine(DroneShimmerLinear(0.001f));
                StartCoroutine(DroneAttack(10, 1));
                break;
            default:
                break;
        }

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
            if (!droneControllers[i].IsAttacking)
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
        activeDrones.Add(drone);
        // Store reference to instance's DroneController component.
        droneControllers.Add(dC);
    }

    static public void RemoveDrone(GameObject _drone)
    {
        if (!activeDrones.Remove(_drone))
            Debug.LogError("HiveController:RemoveDrone(GameObject _drone) - activeDrones removal unsuccessful.");
        if (!droneControllers.Remove(_drone.GetComponent<DroneController>()))
            Debug.LogError("HiveController:RemoveDrone(GameObject _drone) - droneControlls removal unsuccessful.");
    }

    GameObject GetDroneNearestToPlayer()
    {
        float nearestDist = 1000f;
        GameObject nearestDrone = null;
        foreach (GameObject drone in activeDrones)
        {
            float curDist = Vector3.Distance(drone.transform.position, player.transform.position);
            if (curDist < nearestDist)
            {
                nearestDist = curDist;
                nearestDrone = drone;
            }
        }

        return nearestDrone;
    }

    IEnumerator SpawnDrones(float _timeBetweenSpawns)
    {
        for (int i = 0; i < droneCount; i++)
        {
            AddDrone(i, dronePositions[i]);
            yield return new WaitForSeconds(_timeBetweenSpawns);
        }

        SetState(eHiveStates.rotate);
    }

    IEnumerator DroneShimmerLinear(float _timeBetweenShimmers)
    {
        for (int i = 0; i < droneControllers.Count; i += 2)
        {
            droneControllers[i].Shimmer();
            if (i + 1 < droneControllers.Count)
                droneControllers[i + 1].Shimmer();
            yield return new WaitForSeconds(_timeBetweenShimmers);
        }
    }

    IEnumerator DroneAttack(int _numberOfDrones, float _timeBetweenAttacks)
    {
        _numberOfDrones = Mathf.Clamp(_numberOfDrones, 0, activeDrones.Count);
        List<GameObject> attackingDrones = new List<GameObject>();
        Vector3 startRotation = hiveRotationSpeed;
        hiveRotationSpeed = hiveRotationSpeedAttacking;

        for (int i = 0; i < _numberOfDrones; i++)
        {
            GameObject attackDrone = GetDroneNearestToPlayer();
            if (attackDrone != null)
            {
                DroneController dC = attackDrone.GetComponent<DroneController>();
                if (dC != null)
                {
                    dC.Attack(player.transform.position);
                    activeDrones.Remove(dC.gameObject);
                    attackingDrones.Add(dC.gameObject);
                }
            }

            yield return new WaitForSeconds(_timeBetweenAttacks);
        }

        // Add list of attackingDrones back to Hive's activeDrones.
        foreach (GameObject drone in attackingDrones)
        {
            activeDrones.Add(drone);
        }
        attackingDrones.Clear();

        hiveRotationSpeed = startRotation;
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
