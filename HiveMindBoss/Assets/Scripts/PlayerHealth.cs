using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Inscribed")]
    //public Text healthText;
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

    AudioSourceController hurtAudioController;
    AudioSource deathAudio;

    private void Awake()
    {
        hurtAudioController = GameObject.Find("PlayerAudioHurt").GetComponent<AudioSourceController>();
        if (hurtAudioController == null)
            Debug.LogError("PlayerHealth:Awake() - AudioSourceController hurtAudioController is null.");
        deathAudio = GameObject.Find("PlayerAudioDeath").GetComponent<AudioSource>();
        if (deathAudio == null)
            Debug.LogError("PlayerHealth:Awake() - AudioSource deathAudio is null.");
    }

    private void Start()
    {
        panel.SetPlayerHealth(health);
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
                if (drone.IsAlive && drone.IsAttacking)
                {
                    TakeDamage(Hive.DronesSO.attackDamage);
                    if (drone.IsAttacking)
                        drone.IsAttacking = false;
                }
            }
            return;
        }

        if (other.gameObject.tag == "HiveLaser")
        {
            HiveLaser hCL = other.gameObject.GetComponentInParent<HiveLaser>();
            if (hCL != null)
            {
                TakeDamage(hCL.Damage);
                hCL.StopFiring();
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

        panel.SetPlayerHealth(health);

        if (health > 0)
        {
            hurtAudioController.PlayRandomAudioClip();
        }
        else
        {
            Debug.Log("Player has died");
            deathAudio.Play();
            isAlive = false;
        }
    }
}
