using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneGroupLeader : MonoBehaviour
{
    [Header("Inscribed")]
    public float moveSpeedLeaderDroneFactor;
    public Vector3 rotationSpeed;

    public Vector3 TargetPosition { get; set; }
    public int DroneCount { get; set; }
    public bool IsAttacking { get; set; }

    List<Drone> drones = new List<Drone>();
    List<Vector3> dronePositions = new List<Vector3>(); // Set by hive on instantiation.
    float moveSpeed;
    float droneRadius;

    private void Awake()
    {
        IsAttacking = true;
    }

    private void Start()
    {
        moveSpeed = Hive.DronesSO.moveSpeed * moveSpeedLeaderDroneFactor;
        droneRadius = Hive.DronesSO.radius * 3f;

        transform.LookAt(TargetPosition);
        InitializeDronePositions();
    }

    private void Update()
    {
        // Set TargetPosition to hive once original target is reached.
        if (IsAttacking && Vector3.Distance(transform.position, TargetPosition) < 0.01f)
        {
            IsAttacking = false;
            TargetPosition = Hive.CorePosition;
            for (int i = 0; i < drones.Count; i++)
            {
                if (drones[i] != null)
                {
                    if (drones[i].IsAlive)
                    {
                        drones[i].IsAttacking = false;
                        drones[i].IsFollowingLeader = false;
                    }
                }
            }
        }

        // Destroy self once leader returns to hive.
        if (!IsAttacking && Vector3.Distance(transform.position, Hive.CorePosition) < 0.01f)
        {
            Destroy(gameObject);
        }
    }

    private void FixedUpdate()
    {
        UpdatePosition();

        RotateDronePositions();
        UpdateDroneTargetPositions();
    }

    private void OnDestroy()
    {
        // Clear lists.
        dronePositions.Clear();
        drones.Clear();
    }

    void UpdatePosition()
    {
        float step = moveSpeed * Time.fixedDeltaTime;
        transform.position = Vector3.MoveTowards(transform.position, TargetPosition, step);
    }

    void InitializeDronePositions()
    {
        float angleIncrement = 360f / DroneCount;
        Vector3 firstPosition = transform.position + new Vector3(0f, droneRadius, 0f);
        for (int i = 0; i < DroneCount; i++)
        {
            Vector3 dir = firstPosition - transform.position;
            dir = Quaternion.Euler(new Vector3(0, 0, i * angleIncrement)) * dir;
            dronePositions.Add(dir + transform.position);
        }
    }

    void RotateDronePositions()
    {
        Vector3 dir;
        for (int i = 0; i < dronePositions.Count; i++)
        {
            Vector3 rotStep = rotationSpeed * Time.fixedDeltaTime;
            dir = dronePositions[i] - transform.position;
            dir = Quaternion.Euler(rotStep) * dir;
            dronePositions[i] = dir + transform.position;
        }
    }

    void UpdateDroneTargetPositions()
    {
        for (int i = 0; i < drones.Count; i++)
        {
            if (drones[i] != null)
            {
                if (drones[i].IsAlive)
                    drones[i].TargetPosition = dronePositions[i];
            }
        }
    }

    public void AddDrone(Drone _drone)
    {
        if (_drone != null)
        {
            drones.Add(_drone);
            DroneCount = drones.Count;
        }
    }
}
