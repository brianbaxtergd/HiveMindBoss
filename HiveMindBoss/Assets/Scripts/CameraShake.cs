using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    static private CameraShake _S;

    [Header("Inscribed")]
    public float shakeDuration = 0.5f;
    public float shakeMagnitude = 1f;

    bool isShaking = false;
    float activeMagnitude = 0f;

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
        ShakeCamera(S.shakeDuration, S.shakeMagnitude);
    }

    static public void ShakeCamera(float _magnitude)
    {
        ShakeCamera(S.shakeDuration, _magnitude);
    }

    static public void ShakeCamera(float _duration, float _magnitude)
    {
        if (_magnitude >= S.activeMagnitude)
        {
            if (S.isShaking)
            {
                S.StopAllCoroutines();
                S.isShaking = false;
            }
            S.StartCoroutine(S.Shake(_duration, _magnitude));
        }
    }

    IEnumerator Shake(float _duration, float _magnitude)
    {
        isShaking = true;
        activeMagnitude = _magnitude;
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
        activeMagnitude = 0f;
    }
}
