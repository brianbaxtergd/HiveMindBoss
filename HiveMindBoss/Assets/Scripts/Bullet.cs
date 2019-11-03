using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Renderer))]
public class Bullet : MonoBehaviour
{
    [Header("Inscribed")]
    public float speed;
    public float lifeTime;
    public int damage = 20;

    [HideInInspector]
    public static GameObject _bulletAnchor = null;

    Rigidbody rigid;
    Renderer rend;
    [HideInInspector]
    public Vector3 aimDir;
    bool isDestroyed = false; // Sets to true on first collision with enemy. This keeps a single bullet from colliding with multiple enemies if they are overlapping.

    void Start()
    {
        if (_bulletAnchor == null)
            _bulletAnchor = new GameObject("BulletAnchor");
        transform.SetParent(_bulletAnchor.transform);

        transform.LookAt(aimDir);

        rend = GetComponent<Renderer>();
        rigid = GetComponent<Rigidbody>();
        rigid.velocity = transform.forward * speed;

        StartCoroutine(DestroySelfInSeconds(lifeTime));
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Note: Collision with HiveSphere is in HiveSphere script.

        string tag = collision.gameObject.tag;
        if (tag == "Drone" && !isDestroyed)
        {
            isDestroyed = true;
            Drone drone = collision.gameObject.GetComponent<Drone>();
            drone.TakeDamage(damage);
            drone.Shimmer(rend.material.GetColor("_EmissionColor"), Hive.DronesSO.hurtTime);
            Destroy(gameObject);
        }

        if (tag == "HiveCore")
        {
            HiveCore hC = collision.gameObject.GetComponent<HiveCore>();
            if (hC.IsActive)
            {
                hC.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
    }

    IEnumerator DestroySelfInSeconds(float _seconds)
    {
        yield return new WaitForSeconds(_seconds);

        Destroy(gameObject);
    }
}
