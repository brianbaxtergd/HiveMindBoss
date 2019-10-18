using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomScale : MonoBehaviour
{
    [Header("Set in Inspector")]
    [Tooltip("Minimum uniform scale speed for x, y, and z.")]
    public float minScale;
    public float maxScale;
    public float scaleSpeed;

    float scale;
    bool isScalingUp;

    // Start is called before the first frame update
    void Start()
    {
        scale = minScale;
        isScalingUp = true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isScalingUp)
        {
            scale = Mathf.Min(scale + scaleSpeed * Time.deltaTime, maxScale);
            if (scale >= maxScale)
                isScalingUp = false;
        }
        else
        {
            scale = Mathf.Max(scale - scaleSpeed * Time.deltaTime, minScale);
            if (scale <= minScale)
                isScalingUp = true;
        }
        transform.localScale = new Vector3(scale, scale, scale);
    }
}
