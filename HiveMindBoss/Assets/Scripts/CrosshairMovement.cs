using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrosshairMovement : MonoBehaviour
{
    static private CrosshairMovement _S;

    Vector3 mousePoint3D;
    static Transform leftCross;
    static Transform rightCross;
    Transform guns;

    private void Start()
    {
        if (_S == null)
            _S = this;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;

        leftCross = GameObject.Find("CrosshairLeftVert").transform;
        if (leftCross == null)
            Debug.LogError("CrosshairMovement:Start() - leftCross is null.");
        rightCross = GameObject.Find("CrosshairRightVert").transform;
        if (rightCross == null)
            Debug.LogError("CrosshairMovement:Start() - rightCross is null.");

        guns = GameObject.Find("Guns").transform;
        if (guns == null)
            Debug.LogError("CrosshairMovement:Start() - guns is null.");
    }

    private void Update()
    {
        mousePoint3D = Camera.main.ScreenToWorldPoint(Input.mousePosition + Vector3.forward * 20);
        transform.position = mousePoint3D;

        // Point guns at crosshair(s).
        guns.transform.LookAt(transform.position);

        // Unlock cursor with esc key.
        if (Cursor.lockState != CursorLockMode.None)
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
    }

    static public Transform LeftCross
    {
        get { return leftCross; }
    }

    static public Transform RightCross
    {
        get { return rightCross; }
    }
}
