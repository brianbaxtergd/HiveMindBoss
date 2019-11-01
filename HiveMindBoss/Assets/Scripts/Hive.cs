using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hive : MonoBehaviour
{
    static private Hive _S; // Unprotected singleton.

    public enum eHiveStates
    {
        passive,
        idle,
        attackDronesOneByOne,
        attackLaser,
        levelUp,
        dead,
    }
    public enum eHiveTopologies
    {
        sphere,
        layeredSphere,
        doubleRing,
    }

    [Header("Inscribed - Drones")]
    public DronesScriptableObject dronesSO;
    //[Range(10, 250)] public int droneCount;
    [Range(5, 25)] public float droneDistance; // From center of Hive.

    [Header("Inscribed - Hive")]
    public float hiveRotationSpeed;
    public Vector3 hiveRotationIdle;
    public Vector3 hiveRotationAttacking;
    public float passiveStateTime;
    public float idleStateTime;
    public float levelUpStateTime;
    public int[] initialDroneCount;
    public int[] stateAttackDronesOneByOne_droneCountMin;
    public int[] stateAttackDronesOneByOne_droneCountMax;
    public float[] stateAttackDronesOneByOne_timeBetweenDrones;
    public eHiveStates[][] hiveAttackStates; // TODO: Cusomize inspector to allow enum drop-downs in a 2D array (if possible).

    [Header("Dynamic")]
    [SerializeField] int hiveLevel = -1;
    [SerializeField] eHiveStates hiveState;
    [SerializeField] eHiveTopologies hiveTopology;
    [SerializeField] List<Drone> activeDrones;
    [SerializeField] List<Vector3> dronePositions;
    [SerializeField] Vector3 hiveRotation = Vector3.zero;

    float hiveStateTime = 0f;
    int hiveLevelMax = 3; // 3 levels: 0, 1, 2.
    Vector3 hiveRotationTarget = Vector3.zero;
    int droneCount;
    Coroutine attackCoroutine = null;

    GameObject droneAnchor;
    GameObject player;
    HiveCore core;
    HiveLaser laser;
    AudioSource spawnDronesAudio;
    AudioSource levelUpAudio;
    AudioSource deathAudio;

    private void Awake()
    {
        if (_S == null)
            _S = this;

        // Initialize data structures.
        activeDrones = new List<Drone>();
        dronePositions = new List<Vector3>();

        // Find and store references.
        player = GameObject.Find("Player");
        if (player == null)
            Debug.LogError("Hive:Awake() - GameObject player is null.");
        core = GameObject.Find("HiveCore").GetComponent<HiveCore>();
        if (core == null)
            Debug.LogError("Hive:Awake() - HiveCore core is null.");
        laser = GameObject.Find("HiveLaser").GetComponent<HiveLaser>();
        if (laser == null)
            Debug.LogError("Hive:Awake() - HiveLaser laser is null");
        spawnDronesAudio = GameObject.Find("HiveAudioSpawnDrones").GetComponent<AudioSource>();
        if (spawnDronesAudio == null)
            Debug.LogError("Hive:Awake() - AudioSource spawnDronesAudio is null.");
        levelUpAudio = GameObject.Find("HiveAudioLevelUp").GetComponent<AudioSource>();
        if (levelUpAudio == null)
            Debug.LogError("Hive:Awake() - AudioSource levelUpAudio is null.");
        deathAudio = GameObject.Find("HiveAudioDeath").GetComponent<AudioSource>();
        if (deathAudio == null)
            Debug.LogError("Hive:Awake() - AudioSource deathAudio is null.");

        // Spawn empty game object to act as drone anchor in hierarchy.
        droneAnchor = new GameObject("DroneAnchor");
        droneAnchor.transform.SetParent(transform);
    }

    private void Start()
    {
        // Initialize core.
        core.SetHiveCoreShape(HiveCore.eHiveCoreShapes.cube);
        core.Volume = core.Volume;

        // Activate hive.
        hiveTopology = eHiveTopologies.sphere;
        LevelUp();
    }

    private void FixedUpdate()
    {
        switch (hiveState)
        {
            case eHiveStates.passive:
                // Rotate core towards player so it becomes visible.
                float rotStep = 0.25f * Time.fixedDeltaTime;
                Vector3 tarDir = player.transform.position - core.transform.position;
                Vector3 newDir = Vector3.RotateTowards(core.transform.forward, tarDir, rotStep, 0.0f);
                core.transform.LookAt(newDir);
                if (hiveStateTime >= passiveStateTime)
                {
                    SetState(eHiveStates.idle);
                    SpawnDrones();
                    core.SetHiveCoreShape(HiveCore.eHiveCoreShapes.cube);
                    core.Volume = core.Volume;
                }
                break;
            case eHiveStates.idle:
                RotateDronePositions(hiveRotation * Time.fixedDeltaTime);
                if (hiveStateTime >= idleStateTime)
                {
                    eHiveStates[] potentialAttackStates = { eHiveStates.attackDronesOneByOne, eHiveStates.attackLaser };
                    eHiveStates randomAttackState = potentialAttackStates[Random.Range(0, potentialAttackStates.Length)];
                    SetState(randomAttackState);
                }
                break;
            case eHiveStates.attackDronesOneByOne:
                RotateDronePositions(hiveRotation * Time.fixedDeltaTime);
                break;
            case eHiveStates.attackLaser:
                if (!laser.IsFiring)
                {
                    SpawnDrones();
                    SetState(eHiveStates.idle);
                }
                break;
            case eHiveStates.levelUp:
                if (hiveStateTime >= levelUpStateTime)
                {
                    spawnDronesAudio.Play(); // TODO: See comment in SpawnDrones().
                    SpawnDrones();
                    SetState(eHiveStates.idle);
                }
                break;
            case eHiveStates.dead:
                break;
            default:
                break;
        }

        // Update rotation.
        if (hiveRotation != hiveRotationTarget)
        {
            float rotStep = hiveRotationSpeed * Time.fixedDeltaTime;
            hiveRotation = Vector3.RotateTowards(hiveRotation, hiveRotationTarget, rotStep, 1f);
        }

        // Update state timer.
        hiveStateTime += Time.fixedDeltaTime;
    }

    void SetState(eHiveStates _newState)
    {
        // Reset state timer.
        hiveStateTime = 0f;

        // Initialize new state.
        hiveState = _newState;
        switch (hiveState)
        {
            case eHiveStates.passive:
                break;
            case eHiveStates.idle:
                core.IsActive = false;
                hiveRotationTarget = hiveRotationIdle;
                break;
            case eHiveStates.attackDronesOneByOne:
                // Begin sending drones to attack player.
                int randDroneCount = Random.Range(stateAttackDronesOneByOne_droneCountMin[hiveLevel], stateAttackDronesOneByOne_droneCountMax[hiveLevel] + 1);
                attackCoroutine = StartCoroutine(DroneAttack(randDroneCount));
                hiveRotationTarget = hiveRotationAttacking;
                break;
            case eHiveStates.attackLaser:
                KillDrones();
                core.IsActive = true;
                laser.Fire();
                break;
            case eHiveStates.levelUp:
                Debug.Log("Hive leveled up to level: " + hiveLevel + ".");
                levelUpAudio.Play();
                DroneCount = initialDroneCount[hiveLevel];
                break;
            case eHiveStates.dead:
                Debug.Log("Hive has been defeated.");
                // TODO: Find a way to deactivate player movement & combat while still allowing player to tilt back to default orientation.
                // Deactivate player components.
                //player.GetComponent<PlayerMovement>().enabled = false;
                //player.GetComponent<PlayerCombat>().enabled = false;
                // Player death audio.
                deathAudio.Play();
                break;
            default:
                break;
        }
        //hiveState = _newState;
    }

    void LevelUp()
    {
        hiveLevel++;
        eHiveStates nextState = (hiveLevel < hiveLevelMax) ? eHiveStates.levelUp : eHiveStates.dead;
        SetState(nextState);
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
                float offset = 2f / DroneCount;
                float increment = Mathf.PI * (3f - Mathf.Sqrt(5f));
                for (int i = 0; i < DroneCount; i++)
                {
                    float y = ((i * offset) - 1) + (offset / 2f);
                    float r = Mathf.Sqrt(1 - Mathf.Pow(y, 2));
                    float phi = (i % DroneCount) * increment;
                    float x = Mathf.Cos(phi) * r;
                    float z = Mathf.Sin(phi) * r;
                    dronePositions.Add(new Vector3(x, y, z) * droneDistance);
                }
                break;
            case eHiveTopologies.layeredSphere:
                break;
            case eHiveTopologies.doubleRing:
                int totalDrones = DroneCount;
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

    void KillDrones()
    {
        if (activeDrones.Count == 0)
            Debug.LogError("Hive:KillDrones() - called when there are no active drones.");
        //DroneCount = activeDrones.Count;

        // Destroy all drones.
        int newDroneCount = 0;
        foreach (Drone drone in activeDrones)
        {
            if (drone.IsAlive)
            {
                newDroneCount++;
                drone.recalledByHive = true;
                drone.Death();
            }
        }

        // Update droneCount to match number of active drones. This ensures drones killed by player do not respawn if SpawnDrones() coroutine is called following this function.
        DroneCount = newDroneCount;
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

    void SpawnDrones()
    {
        if (activeDrones.Count > 0)
            Debug.LogError("Hive:SpawnDrones() - activeDrones not cleared before spawning new drones.");

        // TODO: Implement this here once eHiveStates attackLaser has its own topology.
        // Play audio.
        //spawnDronesAudio.Play();

        // Redefine dronePositions.
        DefineDronePositions(hiveTopology);

        // Update volume & scale of dronesSO.
        dronesSO.volume = core.Volume / (DroneCount + 1);
        dronesSO.radius = Mathf.Pow((3f * dronesSO.volume) / (4f * Mathf.PI), 1f / 3f);
        dronesSO.scale = new Vector3(dronesSO.radius * 2f, dronesSO.radius * 2f, dronesSO.radius * 2f);

        // Spawn drones.
        for (int i = 0; i < DroneCount; i++)
        {
            Drone drone = Drone.SpawnDrone();
            drone.transform.localScale = dronesSO.scale;
            drone.Index = i;
            drone.Health = dronesSO.initialHealth;
            drone.TargetPosition = dronePositions[i];
            drone.transform.SetParent(droneAnchor.transform);
            drone.gameObject.name = "Drone_" + i.ToString("000");
            activeDrones.Add(drone);
        }

        // Activate core.
        core.IsActive = true;
        // Update core volume.
        core.Volume -= dronesSO.volume * DroneCount;
    }

    IEnumerator DroneAttack(int _droneCount)
    {
        _droneCount = Mathf.Clamp(_droneCount, 0, activeDrones.Count);

        for (int i = 0; i < _droneCount; i++)
        {
            Drone drone = GetDroneNearestToPlayer();
            if (drone != null)
                drone.Attack(player.transform.position);
            yield return new WaitForSeconds(stateAttackDronesOneByOne_timeBetweenDrones[hiveLevel]);
        }

        // Reset attackCoroutine to null.
        attackCoroutine = null;
        // Switch to idle state.
        SetState(eHiveStates.idle);
    }

    /*
    IEnumerator DroneShimmer()
    {
        for (int i = 0; i < activeDrones.Count; i++)
        {
            activeDrones[i].Shimmer();
            yield return null;
        }
    }
    */

    // Properties.

    int DroneCount
    {
        get { return droneCount; }
        set
        {
            droneCount = value;
            if (droneCount == 0)
            {
                // If an attack coroutine is executing, stop it.
                if (attackCoroutine != null)
                    StopCoroutine(attackCoroutine);
                // Level up.
                LevelUp();
            }
        }
    }

    // Statics.

    static public List<Vector3> DronePositions
    {
        get { return _S.dronePositions; }
    }

    static public void RemoveDrone(int _index)
    {
        // Update droneCount if drone was not recalled by hive, but instead destroyed by player.
        if (!_S.activeDrones[_index].recalledByHive)
            _S.DroneCount--;
        // Add drone volume to core.
        _S.core.Volume += _S.core.SphereVolume(_S.activeDrones[_index].transform.localScale.x / 2f);
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

    static public DronesScriptableObject DronesSO
    {
        get { return _S.dronesSO; }
    }
}
