using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Inscribed")]
    public GameObject bulletPrefab;
    public float shotCooldownTime;

    float shotCooldown;
    bool useLeftBarrel = true;
    Transform leftGunBarrel;
    Transform rightGunBarrel;
    Animation leftGunAnim;
    Animation rightGunAnim;

    void Start()
    {
        leftGunBarrel = GameObject.Find("LeftGunBarrel").transform;
        if (leftGunBarrel == null)
            Debug.LogError("PlayerCombat:Start() - GameObject LeftGunBarrel not found.");
        rightGunBarrel = GameObject.Find("RightGunBarrel").transform;
        if (rightGunBarrel == null)
            Debug.LogError("PlayerCombat:Start() - GameObject RightGunBarrel not found.");

        leftGunAnim = GameObject.Find("LeftGun").GetComponent<Animation>();
        if (leftGunAnim == null)
            Debug.LogError("PlayerCombat:Start() - Left gun barrel firing animation not found.");
        rightGunAnim = GameObject.Find("RightGun").GetComponent<Animation>();
        if (rightGunAnim == null)
            Debug.LogError("PlayerCombat:Start() - Right gun barrel firing animation not found.");
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
        GameObject bullet = Instantiate(bulletPrefab);
        bullet.transform.position = spawnPos;
        bullet.transform.rotation = transform.rotation;
        // Play gun barrel firing animation.
        Animation fireAnim = (useLeftBarrel) ? leftGunAnim : rightGunAnim;
        if (fireAnim.isPlaying)
            fireAnim.Stop();
        fireAnim.Play();
        // Swap barrels for next shot.
        useLeftBarrel = !useLeftBarrel;
    }
}
