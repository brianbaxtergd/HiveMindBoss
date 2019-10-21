using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class DroneController : MonoBehaviour
{
    [Header("Inscribed")]
    public int health = 100;
    public float moveSpeed = 1f;
    public int attackDamage = 10;
    public Color hurtColor;
    public float hurtTime;
    public Color shimmerColor;
    [Tooltip("Amount of time in seconds shimmer is active.")]
    public float shimmerTime;
    public float shimmerScale;

    Color defaultColor;
    Vector3 defaultScale;
    bool isAlive = true;
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
        if (isAlive)
            UpdatePosition();
    }

    private void OnDestroy()
    {
        HiveController.RemoveDrone(gameObject);
    }

    void UpdatePosition()
    {
        float step = moveSpeed * Time.fixedDeltaTime;
        transform.position = Vector3.MoveTowards(transform.position, TargetPosition, step);

        if (isAttacking && Vector3.Distance(transform.position, TargetPosition) < 0.01)
            isAttacking = false;
    }

    public void TakeDamage(int _damage)
    {
        if (!isAlive)
            return;

        health = Mathf.Max(health - _damage, 0);
        if (health == 0)
        {
            isAlive = false;

            //Destroy(gameObject);
        }

        // Trigger shimmer effect.
        Shimmer(hurtColor, hurtTime);
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

    public void Shimmer()
    {
        if (isShimmering)
        {
            StopCoroutine("ShimmerColor");
            isShimmering = false;
        }
        StartCoroutine(ShimmerColor(shimmerColor, shimmerTime));
    }

    public void Shimmer(Color _color, float _shimmerTime)
    {
        if (isShimmering)
        {
            StopCoroutine("ShimmerColor");
            isShimmering = false;
        }
        StartCoroutine(ShimmerColor(_color, _shimmerTime));
    }

    IEnumerator ShimmerColor(Color _color, float _shimmerTime)
    {
        isShimmering = true;
        // Set timer values.
        float startTime = Time.time;
        float currTime = startTime;
        float endTime = startTime + _shimmerTime;
        // Change material color.
        Renderer rend = GetComponent<Renderer>();
        rend.material.color = _color;
        yield return new WaitForSeconds(0.1f);
        // Increase scale to max.
        //transform.localScale = defaultScale * shimmerScale;

        // Lerp color back to default.
        do
        {
            rend.material.color = Color.Lerp(_color, defaultColor, Mathf.Clamp((currTime - startTime) / _shimmerTime, 0f, 1f));
            //float newScale = Mathf.Lerp(defaultScale.x * shimmerScale, defaultScale.x, (currTime - startTime) / shimmerTime);
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
    public bool IsAlive
    {
        get { return isAlive; }
        private set { isAlive = value; }
    }
    public bool IsAttacking
    {
        get { return isAttacking; }
        set { isAttacking = value; }
    }
}
