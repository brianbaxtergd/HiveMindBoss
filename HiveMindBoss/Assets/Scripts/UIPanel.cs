using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UIPanel : MonoBehaviour
{
    [Header("Inscribed")]
    public Color damagePanelColor;

    Image panelImage;
    Text playerHealthText;
    HiveHealthScrollbar hiveHealthScrollbar;

    Color defaultColor;
    float damagePanelCooldownTime;

    private void Awake()
    {
        panelImage = GetComponent<Image>();
        playerHealthText = GameObject.Find("PlayerHealthText").GetComponent<Text>();
        if (playerHealthText == null)
            Debug.LogError("UIPanel:Awake() - Text playerHealthText is null.");
        hiveHealthScrollbar = GetComponentInChildren<HiveHealthScrollbar>();
        if (hiveHealthScrollbar == null)
            Debug.LogError("UIPanel:Awake() - HiveHealthScrollbar hiveHealthScrollbar is null.");
    }

    void Start()
    {
        defaultColor = Color.clear;

        PlayerHealth pH = GameObject.Find("Shield").GetComponent<PlayerHealth>();
        if (pH == null)
            Debug.LogError("UIPanel:Start() - pH is null.");
        damagePanelCooldownTime = pH.hurtCooldownTime;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TriggerUIDamage()
    {
        StartCoroutine(DamagePanel(damagePanelCooldownTime));
    }

    public void SetPlayerHealth(int _health)
    {
        playerHealthText.text = "[HP: " + _health + " ]";
    }

    public void SetHiveHealth(int _health, int _maxHealth)
    {
        hiveHealthScrollbar.SetHiveHealth(_health, _maxHealth);
    }

    IEnumerator DamagePanel(float _fadeTime)
    {
        float startTime = Time.time;
        float currTime = startTime;
        float endTime = startTime + damagePanelCooldownTime;
        yield return null;

        do
        {
            currTime = Time.time;
            panelImage.color = Color.Lerp(damagePanelColor, defaultColor, (currTime - startTime) / damagePanelCooldownTime);
            yield return null;
        }
        while (currTime < endTime);

        panelImage.color = defaultColor;
    }
}
