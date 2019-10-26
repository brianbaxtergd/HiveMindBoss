using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugDrawWorldSpaceVectors : MonoBehaviour
{
    Transform player;
    float lineLength;

    void Start()
    {
        player = GameObject.Find("Player").transform;
        if (player == null)
            Debug.LogError("DebugDrawWorldSpaceVectors:Start() - player is null.");
        lineLength = Vector3.Distance(player.position, Hive.CorePosition);
    }

    void Update()
    {
        Debug.DrawLine(Vector3.zero, Vector3.up * lineLength, Color.green);
        Debug.DrawLine(Vector3.zero, Vector3.right * lineLength, Color.red);
        Debug.DrawLine(Vector3.zero, Vector3.forward * lineLength, Color.blue);

        Debug.DrawLine(Vector3.zero, Vector3.down * lineLength, Color.green);
        Debug.DrawLine(Vector3.zero, Vector3.left * lineLength, Color.red);
        Debug.DrawLine(Vector3.zero, Vector3.back * lineLength, Color.blue);
    }
}
