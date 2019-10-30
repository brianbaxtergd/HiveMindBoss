using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(ShimmerColor))]
public class HiveCore : MonoBehaviour
{
    public enum eHiveCoreShapes
    {
        cube,
        rectangularPrism,
    }

    [Header("Inscribed")]
    public float initialVolume;
    public float scaleTime;
    public Vector3 rectangularPrismScaleRatio;
    public Vector3 cubeScaleRatio;
    public float colorChangeTime;
    public Material activeMaterial;
    public Material inactiveMaterial;

    [Header("Dynamic")]
    [SerializeField] eHiveCoreShapes hiveCoreShape;
    [SerializeField] float volume;
    [SerializeField] float scaleFactor = 1f;

    Vector3 targetScale;
    Vector3 scaleSpeed; // This is changed dynamically by smooth damping.
    Renderer rend;
    ShimmerColor shimCol;
    Vector3 scaleRatio;
    bool isActive = false; // Core is "Active" while spawning drones and firing lasers.

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        shimCol = GetComponent<ShimmerColor>();

        SetHiveCoreShape(eHiveCoreShapes.rectangularPrism);
        Volume = initialVolume;
        transform.localScale = targetScale;
    }

    private void Start()
    {

    }

    private void Update()
    {
        if (transform.localScale != targetScale)
        {
            transform.localScale = Vector3.SmoothDamp(transform.localScale, targetScale, ref scaleSpeed, scaleTime);
        }
    }

    public void SetHiveCoreShape(eHiveCoreShapes _newShape)
    {
        hiveCoreShape = _newShape;
        switch (hiveCoreShape)
        {
            case eHiveCoreShapes.cube:
                scaleRatio = cubeScaleRatio;
                break;
            case eHiveCoreShapes.rectangularPrism:
                scaleRatio = rectangularPrismScaleRatio;
                break;
            default:
                break;
        }
    }

    IEnumerator ChangeMaterialColor(Material _curMat, Material _newMat)
    {
        shimCol.DefaultColor = _newMat.GetColor("_EmissionColor");
        float elapsed = 0f;

        while (elapsed < colorChangeTime)
        {
            rend.material.Lerp(_curMat, _newMat, elapsed / colorChangeTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rend.material = _newMat;
    }

    public float Volume
    {
        get { return volume; }
        set
        {
            volume = value;
            // Scale transform to new volume.
            scaleFactor = RectangulrPrismScaleFactor(volume);
            //transform.localScale = scaleRatio * scaleFactor;
            targetScale = scaleRatio * scaleFactor;
        }
    }

    public float SphereVolume(float _radius)
    {
        return ((4f / 3f) * Mathf.PI * Mathf.Pow(_radius, 3));
    }

    public float RectangularPrismVolume(Vector3 _dimensions)
    {
        return (_dimensions.x * _dimensions.y * _dimensions.z);
    }

    public float RectangularPrismVolume(float _length, float _height, float _width)
    {
        return (_length * _height * _width);
    }

    public float RectangulrPrismScaleFactor(float _volume)
    {
        return Mathf.Pow(_volume / RectangularPrismVolume(scaleRatio), 1f/3f);
    }

    // Properties.

    public bool IsActive
    {
        get { return isActive; }
        set
        {
            bool activeCached = isActive;
            isActive = value;
            if (isActive != activeCached)
            {
                if (isActive)
                    StartCoroutine(ChangeMaterialColor(inactiveMaterial, activeMaterial));
                else
                    StartCoroutine(ChangeMaterialColor(activeMaterial, inactiveMaterial));
            }
        }
    }

    // Debugging.

    public void DebugLogCoreVolumeAndScaleFactor()
    {
        Debug.Log("(4, 9, 1) Volume: " + RectangularPrismVolume(scaleRatio * scaleFactor) + " at Scale Factor: " + scaleFactor);
    }
    public void DebugLogCoreScaleFactor(float _volume)
    {
        Debug.Log("Scale factor of volume: " + _volume + ", is equal to: " + RectangulrPrismScaleFactor(_volume));
    }
}
