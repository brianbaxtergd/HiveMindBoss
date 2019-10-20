using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    static private CameraShake _S;

    [Header("Inscribed")]
    public float shakeDuration = 0f;
    public float shakeMagnitude = 0.7f;

    bool isShaking = false;

    private void Awake()
    {
        if (_S == null)
            _S = this;
    }

    static CameraShake S
    {
        get
        {
            if (_S == null)
                Debug.LogError("CameraShake:S getter - Attempt to get _S before it was set.");
            return _S;
        }
        set
        {
            if (_S != null)
                Debug.LogError("CameraShake:S setter - Attempt to redefine _S after it's already been set.");
            _S = value;
        }
    }

    static public void ShakeCamera()
    {
        if (!S.isShaking)
            S.StartCoroutine(S.Shake(S.shakeDuration, S.shakeMagnitude));
    }

    IEnumerator Shake(float _duration, float _magnitude)
    {
        isShaking = true;
        float elapsed = 0f;
        float currMagnitude = _magnitude;

        while(elapsed < _duration)
        {
            transform.position = transform.parent.transform.position + Random.insideUnitSphere * currMagnitude;
            elapsed += Time.deltaTime;
            currMagnitude = Mathf.Lerp(_magnitude, 0f, elapsed / _duration);
            yield return null;
        }

        transform.position = transform.parent.transform.position;
        isShaking = false;
    }
}
