using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))] // This isn't truly required, but it makes the most sense to have this script attached to the main camera.
public class SkyboxChanger : MonoBehaviour
{
    [Header("Inscribed")]
    public Material[] skyboxes;

    int curSkyboxIndex;

    private void Awake()
    {
    }

    private void Start()
    {
        /*
        curSkyboxIndex = 0;
        if (skyboxes.Length > 0)
            RenderSettings.skybox = skyboxes[curSkyboxIndex];
        */

        //RenderSettings.skybox.color = Color.black;
    }
}
