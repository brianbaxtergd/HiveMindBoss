using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Scrollbar))]
public class HiveHealthScrollbar : MonoBehaviour
{
    [Header("Inscribed")]
    public float smoothDampTime;
    public float visibilityTime;
    //public float yPosInvisible;
    //public float yPosVisible;

    Transform visibleTrans;
    Scrollbar scrollbar;
    Vector3 targetPosition;
    Vector3 hiddenPosition;
    Vector3 visiblePosition;
    Vector3 moveSpeed;
    bool isVisible;
    float visibilityTimer = 0f;

    private void Awake()
    {
        visibleTrans = GameObject.Find("HiveHealthScrollbarVisibleTransform").transform;
        if (visibleTrans == null)
            Debug.LogError("HiveHealthScrollbar:Awake() - Transform visibleTrans is null.");
        scrollbar = GetComponent<Scrollbar>();
    }

    private void Start()
    {
        hiddenPosition = transform.position;
        visiblePosition = visibleTrans.position;
        targetPosition = transform.position;
        moveSpeed = Vector2.zero;
    }

    private void Update()
    {
        // Update transform's position toward target.
        if (Mathf.Abs(transform.position.y - targetPosition.y) > 0.01f)
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref moveSpeed, smoothDampTime);

        // Visibility timer.
        if (isVisible)
        {
            // Decrement visibility timer.
            visibilityTimer = Mathf.Max(visibilityTimer - Time.deltaTime, 0f);
            if (visibilityTimer == 0f)
                SetIsVisible(false);
        }
    }

    void SetIsVisible(bool _isVisible)
    {
        // Update targetPosition's y value contingent on visibility.
        isVisible = _isVisible;
        //float newY = (isVisible) ? yPosVisible : yPosInvisible;
        //targetPosition = new Vector3(transform.position.x, newY, 0f);
        targetPosition = (isVisible) ? visiblePosition : hiddenPosition;
    }

    public void SetHiveHealth(int _health, int _maxHealth)
    {
        // Update scrollbar's handle size.
        scrollbar.size = (float)_health / (float)_maxHealth;
        // Make health bar visible.
        SetIsVisible(true);
        // Reset visibility timer.
        visibilityTimer = visibilityTime;
    }
}
