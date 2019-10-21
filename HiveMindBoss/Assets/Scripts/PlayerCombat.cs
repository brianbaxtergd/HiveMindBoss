using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Inscribed")]
    public GameObject bulletPrefab;
    public float shotCooldownTime;
    public float crossHairSpeed = 10f;

    float shotCooldown;
    bool useLeftBarrel = true;
    Transform leftGunBarrel;
    Transform rightGunBarrel;
    Transform crosshair;
    Animation leftGunAnim;
    Animation rightGunAnim;
    AudioSource leftGunAudio;
    AudioSource rightGunAudio;

    void Start()
    {
        leftGunBarrel = GameObject.Find("LeftGunBarrel").transform;
        if (leftGunBarrel == null)
            Debug.LogError("PlayerCombat:Start() - GameObject LeftGunBarrel not found.");
        rightGunBarrel = GameObject.Find("RightGunBarrel").transform;
        if (rightGunBarrel == null)
            Debug.LogError("PlayerCombat:Start() - GameObject RightGunBarrel not found.");

        GameObject leftGun, rightGun;
        leftGun = GameObject.Find("LeftGun");
        rightGun = GameObject.Find("RightGun");

        leftGunAnim = leftGun.GetComponent<Animation>();
        if (leftGunAnim == null)
            Debug.LogError("PlayerCombat:Start() - Left gun barrel firing animation not found.");
        rightGunAnim = rightGun.GetComponent<Animation>();
        if (rightGunAnim == null)
            Debug.LogError("PlayerCombat:Start() - Right gun barrel firing animation not found.");

        leftGunAudio = leftGun.GetComponent<AudioSource>();
        rightGunAudio = rightGun.GetComponent<AudioSource>();
    }

    void Update()
    {
        if (shotCooldown > 0f)
            shotCooldown -= Time.deltaTime;

        if (Input.GetKey(KeyCode.Mouse0))
        {
            if(shotCooldown <= 0)
            {
                FireBullet();
                shotCooldown = shotCooldownTime;
            }
        }
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
        // Swap barrels for next shot.
        useLeftBarrel = !useLeftBarrel;
    }
}
