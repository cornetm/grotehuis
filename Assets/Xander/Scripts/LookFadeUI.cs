using UnityEngine;

public class LookFadeUI : MonoBehaviour
{
    [Header("Look Source (bijv. HeadPivot)")]
    [SerializeField] private Transform lookSource;

    [Header("Focus point (default = dit object)")]
    [SerializeField] private Transform focusPoint;

    [Header("UI Root (CanvasGroup) die gefaded/uitgezet wordt")]
    [SerializeField] private CanvasGroup uiGroup;
    [SerializeField] private GameObject uiRootToDisable; // meestal hetzelfde object als uiGroup.gameObject

    [Header("Gating")]
    [SerializeField] private float maxDistance = 1.8f;
    [SerializeField] private float maxAngle = 12f;

    [Header("Fade")]
    [SerializeField] private float fadeInSpeed = 10f;
    [SerializeField] private float fadeOutSpeed = 8f;
    [SerializeField] private float enableInteractAt = 0.95f;
    [SerializeField] private float disableInteractBelow = 0.80f;
    [SerializeField] private float disableThreshold = 0.01f;

    [Header("Behaviour")]
    [SerializeField] private bool disableUIRootWhenHidden = true;
    [SerializeField] private bool startHidden = true;

    void Reset()
    {
        focusPoint = transform;
    }

    void Awake()
    {
        if (focusPoint == null) focusPoint = transform;

        if (uiGroup != null && uiRootToDisable == null)
            uiRootToDisable = uiGroup.gameObject;

        if (startHidden && uiGroup != null)
        {
            uiGroup.alpha = 0f;
            SetInteractable(false);

            if (disableUIRootWhenHidden && uiRootToDisable != null)
                uiRootToDisable.SetActive(false);
        }
    }

    void Update()
    {
        if (lookSource == null || focusPoint == null || uiGroup == null)
            return;

        bool wantVisible = IsLookingAtFocus();

        // UI aanzetten zodra we willen faden-in (anders kan alpha niet omhoog)
        if (wantVisible && disableUIRootWhenHidden && uiRootToDisable != null && !uiRootToDisable.activeSelf)
            uiRootToDisable.SetActive(true);

        float targetAlpha = wantVisible ? 1f : 0f;
        float speed = wantVisible ? fadeInSpeed : fadeOutSpeed;

        uiGroup.alpha = Mathf.MoveTowards(uiGroup.alpha, targetAlpha, speed * Time.deltaTime);

        // Interactability (slider/text/buttons) netjes aan/uit
        if (uiGroup.alpha >= enableInteractAt)
            SetInteractable(true);
        else if (uiGroup.alpha <= disableInteractBelow)
            SetInteractable(false);

        // Helemaal uit? -> disable UI root (maar trigger blijft aan, dus kan straks weer aan)
        if (!wantVisible && disableUIRootWhenHidden && uiRootToDisable != null && uiGroup.alpha <= disableThreshold)
        {
            uiGroup.alpha = 0f;
            uiRootToDisable.SetActive(false);
        }
    }

    private bool IsLookingAtFocus()
    {
        Vector3 toTarget = focusPoint.position - lookSource.position;
        float dist = toTarget.magnitude;
        if (dist > maxDistance) return false;

        Vector3 dir = toTarget / dist;
        float angle = Vector3.Angle(lookSource.forward, dir);
        return angle <= maxAngle;
    }

    private void SetInteractable(bool on)
    {
        uiGroup.interactable = on;
        uiGroup.blocksRaycasts = on;
    }
}
