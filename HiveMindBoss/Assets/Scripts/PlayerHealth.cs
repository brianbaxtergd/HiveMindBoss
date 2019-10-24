using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Inscribed")]
    public Text healthText;
    public UIPanel panel;
    public int health;
    public float hurtCooldownTime;

    [Header("Dynamic")]
    [SerializeField]
    bool isHurt = false;
    [SerializeField]
    bool isAlive = true;
    [SerializeField]
    float hurtCooldown;

    private void Start()
    {
        healthText.text = "[HP: " + health + " ]";
    }

    void Update()
    {
        if (isHurt)
        {
            hurtCooldown -= Time.deltaTime;
            if (hurtCooldown <= 0f)
            {
                hurtCooldown = 0f;
                isHurt = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Drone")
        {
            Drone drone = other.gameObject.GetComponent<Drone>();
            if (drone != null)
            {
                if (drone.IsAlive)
                {
                    TakeDamage(drone.attackDamage);
                    if (drone.IsAttacking)
                        drone.IsAttacking = false;
                }
            }
            return;
        }

        if (other.gameObject.tag == "HiveCoreLaser")
        {
            HiveCoreLaser hCL = other.gameObject.GetComponentInParent<HiveCoreLaser>();
            if (hCL != null)
            {
                TakeDamage(hCL.Damage);
            }
            return;
        }
    }

    public void TakeDamage(int _damage)
    {
        if (!isAlive || isHurt)
            return;

        isHurt = true;
        hurtCooldown = hurtCooldownTime;
        health = Mathf.Max(health -= _damage, 0);

        panel.TriggerUIDamage();

        CameraShake.ShakeCamera();

        healthText.text = "[HP: " + health + " ]";

        if (health == 0)
        {
            Debug.Log("Player has died");
            isAlive = false;
        }
    }
}
