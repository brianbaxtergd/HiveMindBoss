using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/DronesSO", fileName = "DronesSO.asset")]
[System.Serializable]
public class DronesScriptableObject : ScriptableObject
{
    static public DronesScriptableObject S; // This scriptable object is an unprotected Singleton.

    public DronesScriptableObject()
    {
        S = this;
    }

    [Header("Inscribed")]
    public GameObject dronePrefab;
    public int initialHealth;
    public int attackDamage;
    public float moveSpeed;
    public float hurtTime;
    public float shimmerTime;
    public Color shimmerColor;
    public float colorChangeTime;

    public Material idleMaterial;
    public Material attackMaterial;
    public Material deadMaterial;

    [Header("Dynamic")]
    public float volume;
    public float radius;
    public Vector3 scale;

    public GameObject GetDronePrefab()
    {
        return dronePrefab;
    }

    public float SphereVolume(float _radius)
    {
        return ((4f / 3f) * Mathf.PI * Mathf.Pow(_radius, 3f));
    }
}
