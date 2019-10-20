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
    Color defaultColor;
    float damagePanelCooldownTime;

    // Start is called before the first frame update
    void Start()
    {
        panelImage = GetComponent<Image>();

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
