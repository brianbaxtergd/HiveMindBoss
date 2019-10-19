using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    [Header("Inscribed")]
    public float speed;
    public float lifeTime;

    [HideInInspector]
    public static GameObject _bulletAnchor = null;

    Rigidbody rigid;

    void Start()
    {
        if (_bulletAnchor == null)
            _bulletAnchor = new GameObject("BulletAnchor");
        transform.SetParent(_bulletAnchor.transform);

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
            Debug.Log("Bullet hit drone " + collision.gameObject.GetComponent<DroneController>().Index);
        }
    }

    IEnumerator DestroySelfInSeconds(float _seconds)
    {
        yield return new WaitForSeconds(_seconds);

        Destroy(gameObject);
    }
}
