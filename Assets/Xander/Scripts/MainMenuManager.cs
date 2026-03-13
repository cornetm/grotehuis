using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject mainObject;        // main menu root (default active)
    [SerializeField] private GameObject secondaryObject;   // settings/credits/etc (default inactive)

    [Header("Player Look (must be NeckLook)")]
    [SerializeField] private NeckLook neckLook;            // strictly NeckLook

    [Header("Crosshair")]
    [SerializeField] private Image crosshairDefault;       // dot
    [SerializeField] private Image crosshairInteract;      // hand

    [Header("Raycast")]
    [SerializeField] private Camera rayCamera;
    [SerializeField] private float rayDistance = 3f;
    [SerializeField] private LayerMask interactLayers = ~0;

    [Tooltip("Tag that the raycast target collider must have to be interactable (e.g. Screen, Start, Settings).")]
    [SerializeField] private string interactTag = "Screen";

    [SerializeField] private bool allowTriggerColliders = false;

    [Header("Anti-flicker")]
    [Tooltip("Keeps interact crosshair 'on' for a short time after losing the hit.")]
    [SerializeField] private float hoverGraceTime = 0.08f;

    [Header("Input")]
    [SerializeField] private KeyCode openKey = KeyCode.Mouse0; // LMB
    [SerializeField] private KeyCode closeKey = KeyCode.Escape;

    [Header("Cooldown")]
    [Tooltip("Cooldown (seconds) after opening/closing where LMB/ESC are ignored.")]
    [SerializeField] private float switchCooldownSeconds = 0.25f;

    private bool isHovering;
    private float lastHoverTime;

    private float nextAllowedSwitchTime = 0f;

    private bool SecondaryOpen => secondaryObject != null && secondaryObject.activeSelf;

    void Awake()
    {
        if (rayCamera == null) rayCamera = Camera.main;

        // Start state
        SetPanels(mainActive: true);
        SetCursorLocked(true);

        isHovering = false;
        lastHoverTime = -999f;

        SetCrosshairsVisible(true);
        SetCrosshairMode(interactable: false);

        if (neckLook != null)
            neckLook.enabled = true;

        // allow immediate input on start
        nextAllowedSwitchTime = 0f;
    }

    void Update()
    {
        // Cooldown: blokkeer LMB/ESC en ook crosshair “interact” updates (voelt rustiger)
        bool inCooldown = Time.unscaledTime < nextAllowedSwitchTime;

        if (SecondaryOpen)
        {
            if (!inCooldown && Input.GetKeyDown(closeKey))
                CloseSecondary();

            return;
        }

        // Alleen hover-check als we niet in cooldown zitten
        bool hitNow = !inCooldown && IsLookingAtTaggedBox(interactTag);

        if (hitNow)
            lastHoverTime = Time.unscaledTime;

        bool wantHover = hitNow || (!inCooldown && (Time.unscaledTime - lastHoverTime) <= hoverGraceTime);

        if (wantHover != isHovering)
        {
            isHovering = wantHover;
            SetCrosshairMode(isHovering);
        }

        if (!inCooldown && isHovering && Input.GetKeyDown(openKey))
            OpenSecondary();
    }

    private bool IsLookingAtTaggedBox(string tagName)
    {
        if (rayCamera == null) return false;

        Ray ray = rayCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        var triggerMode = allowTriggerColliders ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore;

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, interactLayers, triggerMode))
        {
            return hit.collider != null
                   && hit.collider is BoxCollider
                   && hit.collider.CompareTag(tagName);
        }

        return false;
    }

    private void OpenSecondary()
    {
        if (Time.unscaledTime < nextAllowedSwitchTime) return;

        SetPanels(mainActive: false);

        // Crosshair off + cursor on
        SetCrosshairsVisible(false);
        SetCursorLocked(false);

        // Disable neck look while in secondary menu
        if (neckLook != null)
            neckLook.enabled = false;

        StartCooldown();
    }

    private void CloseSecondary()
    {
        if (Time.unscaledTime < nextAllowedSwitchTime) return;

        SetPanels(mainActive: true);

        // Crosshair on + cursor off/locked
        SetCrosshairsVisible(true);
        SetCrosshairMode(interactable: false);
        SetCursorLocked(true);

        // Reset hover tracking
        isHovering = false;
        lastHoverTime = -999f;

        // Re-enable neck look
        if (neckLook != null)
            neckLook.enabled = true;

        StartCooldown();
    }

    private void StartCooldown()
    {
        // clamp zodat negatieve values niet raar doen
        float cd = Mathf.Max(0f, switchCooldownSeconds);
        nextAllowedSwitchTime = Time.unscaledTime + cd;
    }

    private void SetPanels(bool mainActive)
    {
        if (mainObject != null) mainObject.SetActive(mainActive);
        if (secondaryObject != null) secondaryObject.SetActive(!mainActive);
    }

    private void SetCrosshairMode(bool interactable)
    {
        if (crosshairDefault != null) crosshairDefault.enabled = !interactable;
        if (crosshairInteract != null) crosshairInteract.enabled = interactable;
    }

    private void SetCrosshairsVisible(bool on)
    {
        if (!on)
        {
            if (crosshairDefault != null) crosshairDefault.enabled = false;
            if (crosshairInteract != null) crosshairInteract.enabled = false;
        }
        else
        {
            // Default visible at first; interact is controlled by SetCrosshairMode
            if (crosshairDefault != null) crosshairDefault.enabled = true;
            if (crosshairInteract != null) crosshairInteract.enabled = false;
        }
    }

    private void SetCursorLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
