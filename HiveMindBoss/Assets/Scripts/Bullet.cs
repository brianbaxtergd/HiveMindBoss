using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    [Header("Inscribed")]
    public float speed;
    public float lifeTime;
    public int damage = 34;

    [HideInInspector]
    public static GameObject _bulletAnchor = null;

    Rigidbody rigid;
    [HideInInspector]
    public Vector3 aimDir;

    void Start()
    {
        if (_bulletAnchor == null)
            _bulletAnchor = new GameObject("BulletAnchor");
        transform.SetParent(_bulletAnchor.transform);

        transform.LookAt(aimDir);

        rigid = GetComponent<Rigidbody>();
        rigid.velocity = transform.forward * speed;

        StartCoroutine(DestroySelfInSeconds(lifeTime));
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Note: Collision with HiveSphere is in HiveSphere script.

        string tag = collision.gameObject.tag;
        if (tag == "Drone")
        {
            collision.gameObject.GetComponent<Drone>().TakeDamage(damage);
            Destroy(gameObject);
        }
    }

    IEnumerator DestroySelfInSeconds(float _seconds)
    {
        yield return new WaitForSeconds(_seconds);

        Destroy(gameObject);
    }
}
