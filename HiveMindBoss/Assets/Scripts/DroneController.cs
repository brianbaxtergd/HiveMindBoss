using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class DroneController : MonoBehaviour
{
    [Header("Inscribed")]
    public float moveSpeed = 1f;
    public int attackDamage = 10;
    public Color shimmerColor;
    [Tooltip("Amount of time in seconds shimmer is active.")]
    public float shimmerTime;
    public float shimmerScale;

    Color defaultColor;
    Vector3 defaultScale;
    bool isAttacking = false;
    bool isShimmering = false;

    int index = 0; // Index position within HiveController's drones list.
    Vector3 targetPosition;

    private void Awake()
    {
        defaultColor = GetComponent<Renderer>().material.color;
        defaultScale = transform.localScale;
    }

    private void FixedUpdate()
    {
        UpdatePosition();
    }

    void UpdatePosition()
    {
        float step = moveSpeed * Time.fixedDeltaTime;
        transform.position = Vector3.MoveTowards(transform.position, TargetPosition, step);

        if (isAttacking && Vector3.Distance(transform.position, TargetPosition) < 0.01)
            isAttacking = false;
    }

    public void Shimmer()
    {
        if (isShimmering)
        {
            StopCoroutine("ShimmerRoutine");
            isShimmering = false;
        }
        StartCoroutine("ShimmerRoutine");
    }

    public void Attack(Vector3 _targetPosition)
    {
        if (isAttacking)
        {
            Debug.Log("DronController:Attack() - Drone sent to attack while already attacking.");
            return;
        }

        isAttacking = true;
        TargetPosition = _targetPosition;
        Shimmer();
    }

    IEnumerator ShimmerRoutine()
    {
        isShimmering = true;
        // Set timer values.
        float startTime = Time.time;
        float currTime = startTime;
        float endTime = startTime + shimmerTime;
        // Change material color.
        Renderer rend = GetComponent<Renderer>();
        rend.material.color = shimmerColor;
        yield return new WaitForSeconds(0.1f);
        // Increase scale to max.
        //transform.localScale = defaultScale * shimmerScale;

        // Lerp color back to default.
        do
        {
            rend.material.color = Color.Lerp(shimmerColor, defaultColor, Mathf.Clamp((currTime - startTime) / shimmerTime, 0f, 1f));
            float newScale = Mathf.Lerp(defaultScale.x * shimmerScale, defaultScale.x, (currTime - startTime) / shimmerTime);
            //transform.localScale = new Vector3(1, 1, 1) * newScale;
            currTime = Time.time;
            yield return null;
        }
        while (currTime < endTime);

        // Reset color to default.
        rend.material.color = defaultColor;
        // Reset scale to default.
        //transform.localScale = defaultScale;

        isShimmering = false;
    }

    public int Index
    {
        get { return index; }
        set { index = value; }
    }
    public Vector3 TargetPosition
    {
        get { return targetPosition; }
        set { targetPosition = value; }
    }
    public bool IsAttacking
    {
        get { return isAttacking; }
        set { isAttacking = value; }
    }
}
