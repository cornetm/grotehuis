using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    [Header("Intro")]
    [SerializeField] private bool playIntroOnStart = true;

    [Tooltip("Extra kleine wacht na scene start, bovenop een paar frames, zodat alles visueel rustig geladen is.")]
    [SerializeField] private float introStartDelay = 0.1f;

    [Tooltip("Hoe lang de tekst in-fade + wit->rood kleurverandering duurt.")]
    [SerializeField] private float introFadeInDuration = 2f;

    [Tooltip("Hoe lang gewacht wordt nadat tekst/raw image volledig zichtbaar zijn.")]
    [SerializeField] private float introHoldDuration = 1f;

    [Tooltip("Hoe lang tekst + raw image weer naar alpha 0 faden.")]
    [SerializeField] private float introFadeOutDuration = 1f;

    [Tooltip("Hoe lang de laatste overlay image van alpha 1 naar 0 faden.")]
    [SerializeField] private float finalOverlayFadeDuration = 1f;

    [Tooltip("Tekst die eerst van alpha 0 naar max gaat en tegelijk van wit naar bloedrood kleurt.")]
    [SerializeField] private TextMeshProUGUI introText;

    [Tooltip("RawImage die tegelijk met de tekst van alpha 0 naar max gaat.")]
    [SerializeField] private RawImage introRawImage;

    [Tooltip("Laatste normale UI Image die na de eerste intro wegfade en daarna uitgaat. Meestal je zwarte overlay.")]
    [SerializeField] private Image finalFadeImage;

    [Tooltip("Eindkleur van de introtekst.")]
    [SerializeField] private Color introTextTargetColor = new Color(0.45f, 0f, 0f, 1f);

    private bool isHovering;
    private float lastHoverTime;
    private float nextAllowedSwitchTime = 0f;

    private bool introPlaying;

    private bool SecondaryOpen => secondaryObject != null && secondaryObject.activeSelf;

    private void Awake()
    {
        if (rayCamera == null) rayCamera = Camera.main;

        // Start state
        SetPanels(mainActive: true);

        isHovering = false;
        lastHoverTime = -999f;

        // Zet UI in een veilige beginstaat
        SetCrosshairsVisible(false);

        if (neckLook != null)
            neckLook.enabled = false;

        // block input until intro klaar is
        introPlaying = playIntroOnStart;
        nextAllowedSwitchTime = float.MaxValue;

        PrepareIntroVisualState();
    }

    private void Start()
    {
        if (playIntroOnStart)
        {
            StartCoroutine(PlayIntroSequence());
        }
        else
        {
            FinishIntroAndEnableMenu();
        }
    }

    private void Update()
    {
        if (introPlaying)
            return;

        bool inCooldown = Time.unscaledTime < nextAllowedSwitchTime;

        if (SecondaryOpen)
        {
            if (!inCooldown && Input.GetKeyDown(closeKey))
                CloseSecondary();

            return;
        }

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

    private IEnumerator PlayIntroSequence()
    {
        // Wacht een paar frames zodat canvas/layout/materials/camera rustig staan
        yield return null;
        yield return new WaitForEndOfFrame();
        yield return null;

        if (introStartDelay > 0f)
            yield return new WaitForSecondsRealtime(introStartDelay);

        // 1) Tekst alpha 0 -> 1, wit -> bloedrood
        //    Raw image alpha 0 -> 1
        float t = 0f;
        float fadeInDur = Mathf.Max(0.001f, introFadeInDuration);

        while (t < fadeInDur)
        {
            float lerp = t / fadeInDur;

            if (introText != null)
            {
                Color c = Color.Lerp(Color.white, introTextTargetColor, lerp);
                c.a = Mathf.Lerp(0f, 1f, lerp);
                introText.color = c;
            }

            if (introRawImage != null)
            {
                SetGraphicAlpha(introRawImage, Mathf.Lerp(0f, 1f, lerp));
            }

            t += Time.unscaledDeltaTime;
            yield return null;
        }

        if (introText != null)
        {
            Color c = introTextTargetColor;
            c.a = 1f;
            introText.color = c;
        }

        if (introRawImage != null)
        {
            SetGraphicAlpha(introRawImage, 1f);
        }

        // 2) Kort wachten
        if (introHoldDuration > 0f)
            yield return new WaitForSecondsRealtime(introHoldDuration);

        // 3) Tekst + raw image naar alpha 0
        t = 0f;
        float fadeOutDur = Mathf.Max(0.001f, introFadeOutDuration);

        Color textColorAtFadeStart = introText != null ? introText.color : introTextTargetColor;

        while (t < fadeOutDur)
        {
            float lerp = t / fadeOutDur;
            float alpha = Mathf.Lerp(1f, 0f, lerp);

            if (introText != null)
            {
                Color c = textColorAtFadeStart;
                c.a = alpha;
                introText.color = c;
            }

            if (introRawImage != null)
            {
                SetGraphicAlpha(introRawImage, alpha);
            }

            t += Time.unscaledDeltaTime;
            yield return null;
        }

        if (introText != null)
        {
            Color c = introText.color;
            c.a = 0f;
            introText.color = c;
        }

        if (introRawImage != null)
        {
            SetGraphicAlpha(introRawImage, 0f);
        }

        // 4) Laatste image van alpha 1 -> 0, daarna uitzetten
        if (finalFadeImage != null)
        {
            if (!finalFadeImage.gameObject.activeSelf)
                finalFadeImage.gameObject.SetActive(true);

            SetGraphicAlpha(finalFadeImage, 1f);

            t = 0f;
            float finalFadeDur = Mathf.Max(0.001f, finalOverlayFadeDuration);

            while (t < finalFadeDur)
            {
                float lerp = t / finalFadeDur;
                SetGraphicAlpha(finalFadeImage, Mathf.Lerp(1f, 0f, lerp));

                t += Time.unscaledDeltaTime;
                yield return null;
            }

            SetGraphicAlpha(finalFadeImage, 0f);
            finalFadeImage.gameObject.SetActive(false);
        }

        FinishIntroAndEnableMenu();
    }

    private void FinishIntroAndEnableMenu()
    {
        introPlaying = false;

        SetCursorLocked(true);
        SetCrosshairsVisible(true);
        SetCrosshairMode(interactable: false);

        if (neckLook != null)
            neckLook.enabled = true;

        isHovering = false;
        lastHoverTime = -999f;

        nextAllowedSwitchTime = 0f;
    }

    private void PrepareIntroVisualState()
    {
        if (introText != null)
        {
            Color c = Color.white;
            c.a = 0f;
            introText.color = c;
        }

        if (introRawImage != null)
        {
            SetGraphicAlpha(introRawImage, 0f);
        }

        if (finalFadeImage != null)
        {
            if (!finalFadeImage.gameObject.activeSelf)
                finalFadeImage.gameObject.SetActive(true);

            SetGraphicAlpha(finalFadeImage, 1f);
        }
    }

    private bool IsLookingAtTaggedBox(string tagName)
    {
        if (rayCamera == null) return false;

        Ray ray = rayCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        QueryTriggerInteraction triggerMode = allowTriggerColliders
            ? QueryTriggerInteraction.Collide
            : QueryTriggerInteraction.Ignore;

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

        SetCrosshairsVisible(false);
        SetCursorLocked(false);

        if (neckLook != null)
            neckLook.enabled = false;

        StartCooldown();
    }

    private void CloseSecondary()
    {
        if (Time.unscaledTime < nextAllowedSwitchTime) return;

        SetPanels(mainActive: true);

        SetCrosshairsVisible(true);
        SetCrosshairMode(interactable: false);
        SetCursorLocked(true);

        isHovering = false;
        lastHoverTime = -999f;

        if (neckLook != null)
            neckLook.enabled = true;

        StartCooldown();
    }

    private void StartCooldown()
    {
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
            if (crosshairDefault != null) crosshairDefault.enabled = true;
            if (crosshairInteract != null) crosshairInteract.enabled = false;
        }
    }

    private void SetCursorLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    private static void SetGraphicAlpha(Graphic graphic, float alpha)
    {
        if (graphic == null) return;

        Color c = graphic.color;
        c.a = alpha;
        graphic.color = c;
    }
}