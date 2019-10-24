using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class ShimmerColor : MonoBehaviour
{
    [Header("Inscribed")]
    public float shimmerTime = 1f;

    bool isShimmering;
    Color defaultColor;
    Renderer rend;
    Coroutine curRoutine;

    private void Start()
    {
        rend = GetComponent<Renderer>();
        defaultColor = rend.material.GetColor("_EmissionColor");
    }

    public void Shimmer(Color _color)
    {
        if (isShimmering)
        {
            StopCoroutine(curRoutine);
            isShimmering = false;
        }
        curRoutine = StartCoroutine(ShimmerRoutine(_color, shimmerTime));
    }

    public void Shimmer(Color _color, float _shimmerTime)
    {
        if (isShimmering)
        {
            StopCoroutine(curRoutine);
            isShimmering = false;
        }
        curRoutine = StartCoroutine(ShimmerRoutine(_color, _shimmerTime));
    }

    IEnumerator ShimmerRoutine(Color _color, float _shimmerTime)
    {
        isShimmering = true;

        float elapsed = 0f;

        // Change material color.
        Renderer rend = GetComponent<Renderer>();
        rend.material.SetColor("_EmissionColor", _color);

        yield return null;

        // Lerp color back to default.
        while(elapsed < _shimmerTime)
        {
            rend.material.SetColor("_EmissionColor", Color.Lerp(_color, defaultColor, Mathf.Clamp(elapsed / _shimmerTime, 0f, 1f)));
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Reset color to default.
        rend.material.SetColor("_EmissionColor", defaultColor);

        isShimmering = false;
    }
}
