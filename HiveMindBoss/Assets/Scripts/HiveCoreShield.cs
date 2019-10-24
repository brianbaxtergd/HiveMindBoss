using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ShimmerColor))]
public class HiveCoreShield : MonoBehaviour
{
    [Header("Inscribed")]
    public float deflectTime;

    Renderer rend;
    ShimmerColor shimCol;

    // Start is called before the first frame update
    void Start()
    {
        rend = GetComponent<Renderer>();
        shimCol = GetComponent<ShimmerColor>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        string tag = collision.gameObject.tag;
        if (tag == "Bullet")
        {
            shimCol.Shimmer(
                collision.gameObject.GetComponent<Renderer>().material.GetColor("_EmissionColor"),
                deflectTime);
            Destroy(collision.gameObject);
        }
    }
}
