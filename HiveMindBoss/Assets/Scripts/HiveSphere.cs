using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class HiveSphere : MonoBehaviour
{
    [Header("Inscribed")]
    public float deflectCooldown;

    Renderer rend;
    Color defaultColor;

    // Start is called before the first frame update
    void Start()
    {
        rend = GetComponent<Renderer>();
        defaultColor = rend.material.color;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        string tag = collision.gameObject.tag;
        if (tag == "Bullet")
        {
            DeflectEffect(collision.gameObject.GetComponent<Renderer>().material.color);
            Destroy(collision.gameObject);
        }
    }

    public void DeflectEffect(Color _deflectionColor)
    {
        StartCoroutine(DeflectEffectRoutine(_deflectionColor));
    }

    IEnumerator DeflectEffectRoutine(Color _deflectionColor)
    {
        rend.material.color = _deflectionColor;
        float startTime = Time.time;
        float currTime = startTime;
        float endTime = startTime + deflectCooldown;

        while(currTime < endTime)
        {
            currTime = Time.time;
            rend.material.color = Color.Lerp(_deflectionColor, defaultColor, Mathf.Clamp((currTime - startTime) / deflectCooldown, 0, 1));
            yield return null;
        }

        rend.material.color = defaultColor;
    }
}
