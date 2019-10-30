using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Inscribed")]
    public GameObject bulletPrefab;
    public float shotCooldownTime;
    public float shotCameraShakeMagnitude;
    public float shotCameraShakeDuration;
    public Material barrelCoolMaterial;
    public Material barrelHeatMaterial;
    public float overheatTime;

    [Header("Dynamic")]
    [SerializeField] bool isOverheating = false;
    [SerializeField] float heatLevel = 0f;

    float shotCooldown;
    float overheatCooldown;
    float heatLevelMax = 1f;
    float heatLevelInc;
    float heatLevelDec;
    bool useLeftBarrel = true;
    Transform leftGunBarrel;
    Transform rightGunBarrel;
    Transform crosshair;
    Animation leftGunAnim;
    Animation rightGunAnim;
    AudioSource leftGunAudio;
    AudioSource rightGunAudio;
    AudioSource overheatAudio;
    Renderer leftGunBarrelRend;
    Renderer rightGunBarrelRend;

    private void Awake()
    {
        leftGunBarrel = GameObject.Find("LeftGunBarrel").transform;
        if (leftGunBarrel == null)
            Debug.LogError("PlayerCombat:Awake() - GameObject leftGunBarrel is null.");
        rightGunBarrel = GameObject.Find("RightGunBarrel").transform;
        if (rightGunBarrel == null)
            Debug.LogError("PlayerCombat:Awake() - GameObject RightGunBarrel is null.");

        GameObject leftGun, rightGun;
        leftGun = GameObject.Find("LeftGun");
        rightGun = GameObject.Find("RightGun");

        leftGunAnim = leftGun.GetComponent<Animation>();
        if (leftGunAnim == null)
            Debug.LogError("PlayerCombat:Awake() - Animation leftGunAnim is null.");
        rightGunAnim = rightGun.GetComponent<Animation>();
        if (rightGunAnim == null)
            Debug.LogError("PlayerCombat:Awake() - Animation rightGunAnim is null.");

        leftGunAudio = leftGun.GetComponent<AudioSource>();
        if (leftGunAudio == null)
            Debug.LogError("PlayerCombat:Awake() - AudioSource leftGunAudio is null.");
        rightGunAudio = rightGun.GetComponent<AudioSource>();
        if (rightGunAudio == null)
            Debug.LogError("PlayerCombat:Awake() - AudioSource rightGunAudio is null.");
        overheatAudio = GameObject.Find("Guns").GetComponent<AudioSource>();
        if (overheatAudio == null)
            Debug.LogError("PlayerCombat:Awake() - AudioSource overheatAudio is null.");

        leftGunBarrelRend = leftGunBarrel.GetComponent<Renderer>();
        if (leftGunBarrelRend == null)
            Debug.LogError("PlayerCombat:Awake() - Renderer leftGunBarrelRend is null.");
        rightGunBarrelRend = rightGunBarrel.GetComponent<Renderer>();
        if (rightGunBarrelRend == null)
            Debug.LogError("PlayerCombat:Awake() - Renderer rightGunBarrelRend is null.");
    }

    void Start()
    {
        float numOfShotsToOverheat = 30f;
        heatLevelInc = heatLevelMax / numOfShotsToOverheat;
        heatLevelDec = (heatLevelMax / (shotCooldownTime * numOfShotsToOverheat)) * 3f;
    }

    void Update()
    {
        // Shot cooldown timer.
        if (shotCooldown > 0f)
            shotCooldown -= Time.deltaTime;

        // Overheat cooldown timer.
        if (isOverheating)
        {
            overheatCooldown = Mathf.Max(overheatCooldown - Time.deltaTime, 0f);
            heatLevel = overheatCooldown / overheatTime;
            if (overheatCooldown == 0f)
                isOverheating = false;
        }
        else
        {
            if (Input.GetKey(KeyCode.Mouse0))
            {
                // Firing.
                if (shotCooldown <= 0)
                {
                    FireBullet();
                    shotCooldown = shotCooldownTime;
                }
            }
        }

        // Update gun barrel material.
        leftGunBarrelRend.material.Lerp(barrelCoolMaterial, barrelHeatMaterial, heatLevel / heatLevelMax);
        rightGunBarrelRend.material.Lerp(barrelCoolMaterial, barrelHeatMaterial, heatLevel / heatLevelMax);
    }

    private void FixedUpdate()
    {
        // Decrement heat.
        if (!isOverheating && !Input.GetKey(KeyCode.Mouse0))
            heatLevel = Mathf.Max(heatLevel - heatLevelDec * Time.fixedDeltaTime, 0);
    }

    void FireBullet()
    {
        // Instantiate a bullet.
        Vector3 spawnPos = (useLeftBarrel) ? leftGunBarrel.position : rightGunBarrel.position;
        Vector3 lookDir = (useLeftBarrel) ? leftGunBarrel.forward : rightGunBarrel.forward;
        GameObject bullet = Instantiate(bulletPrefab);
        bullet.transform.position = spawnPos;
        bullet.GetComponent<Bullet>().aimDir = (useLeftBarrel) ? CrosshairMovement.LeftCross.position : CrosshairMovement.RightCross.position;
        // Play gun barrel firing animation.
        Animation fireAnim = (useLeftBarrel) ? leftGunAnim : rightGunAnim;
        if (fireAnim.isPlaying)
            fireAnim.Stop();
        fireAnim.Play();
        // Play fire audio.
        AudioSource fireAudio = (useLeftBarrel) ? leftGunAudio : rightGunAudio;
        fireAudio.Play();
        // Shake camera.
        CameraShake.ShakeCamera(shotCameraShakeDuration, shotCameraShakeMagnitude);
        // Swap barrels for next shot.
        useLeftBarrel = !useLeftBarrel;
        // Update heatLevel and check for overheating.
        heatLevel = Mathf.Min(heatLevel + heatLevelInc, heatLevelMax);
        if (heatLevel == heatLevelMax)
        {
            isOverheating = true;
            overheatCooldown = overheatTime;
            overheatAudio.Play();
        }
    }
}
