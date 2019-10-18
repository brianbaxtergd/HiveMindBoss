using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiveController : MonoBehaviour
{
    public enum eHiveStates
    {
        rotate,
    }

    [Header("Set in Inspector")]
    public int droneCount;
    public float droneRadius;
    public float droneTimeBetweenSpawns;
    [Tooltip("Measured in degrees per second.")]
    public float hiveRotationSpeed;
    public GameObject cube;
    public GameObject sphere;
    public GameObject droneAnchor;
    public GameObject dronePrefab;

    List<GameObject> drones;
    List<Vector3> dronePositions;
    eHiveStates hiveState;
    float hiveAngle = 0f;

    // Start is called before the first frame update
    private void Start()
    {
        drones = new List<GameObject>();
        dronePositions = new List<Vector3>();

        InitializeDronePositions(droneCount, droneRadius, false);

        StartCoroutine(SpawnDrones(droneTimeBetweenSpawns));

        SetState(eHiveStates.rotate);
    }

    // Update is called once per frame
    private void Update()
    {
        //DebugDrawDronePositions();
    }

    private void FixedUpdate()
    {
        switch (hiveState)
        {
            case eHiveStates.rotate:
                //transform.Rotate(0f, 1f, 0f, Space.Self);
                for (int i = 0; i < drones.Count; i++)
                {
                    drones[i].transform.RotateAround(transform.up, hiveAngle + hiveRotationSpeed * Time.fixedDeltaTime);
                    drones[i].transform.Rotate()
                }
                break;
            default:
                break;
        }
        // Update dronePositions.
        InitializeDronePositions(droneCount, droneRadius, false);
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

    void AddDrone(Vector3 _targetPosition)
    {
        // Initialize instance of drone prefab.
        GameObject drone = Instantiate(dronePrefab);
        //drone.transform.position = transform.position;
        drone.transform.SetParent(droneAnchor.transform);
        drone.GetComponent<DroneMovement>().TargetPosition = _targetPosition;
        // Store reference to instance.
        drones.Add(drone);
    }

    IEnumerator SpawnDrones(float _timeBetweenSpawns)
    {
        for (int i = 0; i < droneCount; i++)
        {
            AddDrone(dronePositions[i]);
            yield return new WaitForSeconds(_timeBetweenSpawns);
        }
    }

    void DebugDrawDronePositions()
    {
        for (int i = 0; i < dronePositions.Count; i++)
        {
            Debug.DrawLine(transform.position, dronePositions[i], Color.white);
        }
    }
}
