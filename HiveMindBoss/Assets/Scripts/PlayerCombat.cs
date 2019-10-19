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

    void Start()
    {
        leftGunBarrel = GameObject.Find("LeftGunBarrel").transform;
        if (leftGunBarrel == null)
            Debug.LogError("PlayerCombat:Start() - GameObject LeftGunBarrel not found.");
        rightGunBarrel = GameObject.Find("RightGunBarrel").transform;
        if (rightGunBarrel == null)
            Debug.LogError("PlayerCombat:Start() - GameObject RightGunBarrel not found.");
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
        Vector3 spawnPos = (useLeftBarrel) ? leftGunBarrel.position : rightGunBarrel.position;
        GameObject bullet = Instantiate(bulletPrefab);
        bullet.transform.position = spawnPos;
        bullet.transform.rotation = transform.rotation;

        useLeftBarrel = !useLeftBarrel;
    }
}
