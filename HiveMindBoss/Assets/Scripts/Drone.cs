using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(ShimmerColor))]
public class Drone : MonoBehaviour
{
    [Header("Inscribed")]
    public int health;
    public float moveSpeed;
    public int attackDamage;
    public Color hurtColor;
    public float hurtTime;
    public Color shimmerColor;
    public float shimmerTime;

    bool isAlive = true;
    bool isAttacking = false;

    int index = 0; // Index position within Hive's activeDrones list.
    Vector3 targetPosition;
    Renderer rend;
    ShimmerColor shimCol;
    Color defaultColor;

    private void Start()
    {
        rend = GetComponent<Renderer>();
        shimCol = GetComponent<ShimmerColor>();

        defaultColor = rend.material.GetColor("_EmissionColor");
    }

    private void FixedUpdate()
    {
        UpdateTargetPosition();
        UpdatePosition();

        if (isAlive)
        {
            if (isAttacking && Vector3.Distance(transform.position, targetPosition) < 0.01f)
                isAttacking = false;
        }
        else
        {
            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
                Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        Hive.RemoveDrone(index);
    }

    void UpdateTargetPosition()
    {
        if (isAlive)
        {
            if (!isAttacking)
                targetPosition = Hive.DronePositions[index];
        }
        else
        {
            targetPosition = Hive.CorePosition;
        }
    }

    void UpdatePosition()
    {
        // Move toward target position.
        float step = moveSpeed * Time.fixedDeltaTime;
        transform.position = Vector3.MoveTowards(transform.position, TargetPosition, step);
    }

    public void TakeDamage(int _damage)
    {
        if (!isAlive)
            return;

        health = Mathf.Max(health - _damage, 0);
        if (health == 0)
            Death();

        // Trigger shimmer effect.
        Shimmer(hurtColor, hurtTime);
    }

    private void Death()
    {
        isAlive = false;
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
        shimCol.Shimmer(shimmerColor, shimmerTime);
    }

    public void Shimmer(Color _color, float _shimmerTime)
    {
        shimCol.Shimmer(_color, _shimmerTime);
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
    public Color DefaultColor
    {
        get { return defaultColor; }
        set { defaultColor = value; }
    }
}
