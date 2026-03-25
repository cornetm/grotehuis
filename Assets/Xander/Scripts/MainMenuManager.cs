using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    private enum PromptType
    {
        None,
        StartGame,
        QuitGame
    }

    [Header("UI Panels")]
    [SerializeField] private GameObject mainObject;
    [SerializeField] private GameObject secondaryObject;

    [Header("Player Look (must be NeckLook)")]
    [SerializeField] private NeckLook neckLook;

    [Header("Crosshair")]
    [SerializeField] private Image crosshairDefault;
    [SerializeField] private Image crosshairInteract;

    [Header("Raycast")]
    [SerializeField] private Camera rayCamera;
    [SerializeField] private float rayDistance = 3f;
    [SerializeField] private LayerMask interactLayers = ~0;

    [Tooltip("Tag for regular secondary menu interact objects.")]
    [SerializeField] private string interactTag = "Screen";

    [SerializeField] private bool allowTriggerColliders = false;

    [Header("Anti-flicker")]
    [SerializeField] private float hoverGraceTime = 0.08f;

    [Header("Input")]
    [SerializeField] private KeyCode openKey = KeyCode.Mouse0;
    [SerializeField] private KeyCode closeKey = KeyCode.Escape;

    [Header("Cooldown")]
    [SerializeField] private float switchCooldownSeconds = 0.25f;

    [Header("Intro")]
    [SerializeField] private bool playIntroOnStart = true;
    [SerializeField] private float introStartDelay = 0.1f;
    [SerializeField] private float introFadeInDuration = 2f;
    [SerializeField] private float introHoldDuration = 1f;
    [SerializeField] private float introFadeOutDuration = 1f;
    [SerializeField] private float finalOverlayFadeDuration = 1f;
    [SerializeField] private TextMeshProUGUI introText;
    [SerializeField] private RawImage introRawImage;
    [SerializeField] private Image finalFadeImage;
    [SerializeField] private Color introTextTargetColor = new Color(0.45f, 0f, 0f, 1f);

    [Header("Post Intro Activation")]
    [SerializeField] private NeckLook neckLookAfterIntro;
    [SerializeField] private GameObject gameObjectAfterIntro;

    [Header("Optional Car Crash Flow")]
    [SerializeField] private bool carCrash = false;
    [SerializeField] private string startGameTag = "StartGame";
    [SerializeField] private GameObject startGamePromptObject;
    [SerializeField] private GameObject startGameLeftMouseObject;
    [SerializeField] private GameObject startGameRightMouseObject;
    [SerializeField] private RoadTileManager roadTileManager;

    [Header("Optional Quit Game Flow")]
    [SerializeField] private bool quitGame = false;
    [SerializeField] private string quitGameTag = "QuitGame";
    [SerializeField] private GameObject quitGamePromptObject;
    [SerializeField] private GameObject quitGameLeftMouseObject;
    [SerializeField] private GameObject quitGameRightMouseObject;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = false;

    private bool isHovering;
    private float lastHoverTime;
    private float nextAllowedSwitchTime = 0f;

    private bool introPlaying;
    private PromptType currentPrompt = PromptType.None;

    private bool SecondaryOpen => secondaryObject != null && secondaryObject.activeSelf;
    private bool AnyPromptOpen => currentPrompt != PromptType.None;

    private void Awake()
    {
        if (rayCamera == null) rayCamera = Camera.main;

        SetPanels(mainActive: true);

        isHovering = false;
        lastHoverTime = -999f;

        SetCrosshairsVisible(false);

        if (neckLook != null)
            neckLook.enabled = false;

        introPlaying = playIntroOnStart;
        nextAllowedSwitchTime = float.MaxValue;

        PrepareIntroVisualState();
        PreparePromptVisualStates();

        if (neckLookAfterIntro != null)
            neckLookAfterIntro.enabled = false;

        if (gameObjectAfterIntro != null)
            gameObjectAfterIntro.SetActive(false);
    }

    private void Start()
    {
        if (playIntroOnStart)
            StartCoroutine(PlayIntroSequence());
        else
            FinishIntroAndEnableMenu();
    }

    private void Update()
    {
        if (introPlaying)
            return;

        bool inCooldown = Time.unscaledTime < nextAllowedSwitchTime;

        if (AnyPromptOpen)
        {
            HandlePromptInput(inCooldown);
            return;
        }

        if (SecondaryOpen)
        {
            if (!inCooldown && Input.GetKeyDown(closeKey))
            {
                Log("Closing secondary menu.");
                CloseSecondary();
            }

            return;
        }

        bool hitAnyInteract = false;
        bool hitScreen = false;
        bool hitStartGame = false;
        bool hitQuitGame = false;

        if (!inCooldown)
        {
            GetLookHitState(out hitAnyInteract, out hitScreen, out hitStartGame, out hitQuitGame);
        }

        if (hitAnyInteract)
            lastHoverTime = Time.unscaledTime;

        bool wantHover = hitAnyInteract || (!inCooldown && (Time.unscaledTime - lastHoverTime) <= hoverGraceTime);

        if (wantHover != isHovering)
        {
            isHovering = wantHover;
            SetCrosshairMode(isHovering);
        }

        if (!inCooldown && Input.GetKeyDown(openKey))
        {
            Log($"LMB pressed. hitAny={hitAnyInteract}, hitScreen={hitScreen}, hitStartGame={hitStartGame}, hitQuitGame={hitQuitGame}, carCrash={carCrash}, quitGame={quitGame}");

            if (carCrash && hitStartGame)
            {
                Log("StartGame hit detected -> opening start prompt.");
                OpenStartGamePrompt();
                return;
            }

            if (quitGame && hitQuitGame)
            {
                Log("QuitGame hit detected -> opening quit prompt.");
                OpenQuitGamePrompt();
                return;
            }

            if (hitScreen)
            {
                Log("Screen hit detected -> opening secondary menu.");
                OpenSecondary();
                return;
            }

            Log("LMB pressed but no valid interact target was confirmed.");
        }
    }

    private void HandlePromptInput(bool inCooldown)
    {
        if (inCooldown)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            Log($"{currentPrompt} prompt open -> LMB pressed (confirm).");
            ConfirmCurrentPrompt();
            return;
        }

        if (Input.GetMouseButtonDown(1))
        {
            Log($"{currentPrompt} prompt open -> RMB pressed (cancel).");
            CancelCurrentPrompt();
        }
    }

    private void GetLookHitState(out bool hitAnyInteract, out bool hitScreen, out bool hitStartGame, out bool hitQuitGame)
    {
        hitAnyInteract = false;
        hitScreen = false;
        hitStartGame = false;
        hitQuitGame = false;

        if (rayCamera == null)
        {
            Debug.LogWarning("[MainMenuManager] No rayCamera assigned.");
            return;
        }

        Ray ray = rayCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        QueryTriggerInteraction triggerMode = allowTriggerColliders
            ? QueryTriggerInteraction.Collide
            : QueryTriggerInteraction.Ignore;

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, interactLayers, triggerMode))
        {
            if (hit.collider == null)
                return;

            Log($"Raycast hit: {hit.collider.name} | tag={hit.collider.tag} | type={hit.collider.GetType().Name}");

            if (!string.IsNullOrEmpty(interactTag) && hit.collider.CompareTag(interactTag))
            {
                hitAnyInteract = true;
                hitScreen = true;
                Log("Hit interactTag target.");
            }

            if (carCrash && !string.IsNullOrEmpty(startGameTag) && hit.collider.CompareTag(startGameTag))
            {
                hitAnyInteract = true;
                hitStartGame = true;
                Log("Hit StartGame tag target.");
            }

            if (quitGame && !string.IsNullOrEmpty(quitGameTag) && hit.collider.CompareTag(quitGameTag))
            {
                hitAnyInteract = true;
                hitQuitGame = true;
                Log("Hit QuitGame tag target.");
            }
        }
    }

    private IEnumerator PlayIntroSequence()
    {
        yield return null;
        yield return new WaitForEndOfFrame();
        yield return null;

        if (introStartDelay > 0f)
            yield return new WaitForSecondsRealtime(introStartDelay);

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
                SetGraphicAlpha(introRawImage, Mathf.Lerp(0f, 1f, lerp));

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
            SetGraphicAlpha(introRawImage, 1f);

        if (introHoldDuration > 0f)
            yield return new WaitForSecondsRealtime(introHoldDuration);

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
                SetGraphicAlpha(introRawImage, alpha);

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
            SetGraphicAlpha(introRawImage, 0f);

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
        SetCrosshairMode(false);

        if (neckLook != null)
            neckLook.enabled = true;

        isHovering = false;
        lastHoverTime = -999f;
        nextAllowedSwitchTime = 0f;

        if (neckLookAfterIntro != null)
            neckLookAfterIntro.enabled = true;

        if (gameObjectAfterIntro != null)
            gameObjectAfterIntro.SetActive(true);
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
            SetGraphicAlpha(introRawImage, 0f);

        if (finalFadeImage != null)
        {
            if (!finalFadeImage.gameObject.activeSelf)
                finalFadeImage.gameObject.SetActive(true);

            SetGraphicAlpha(finalFadeImage, 1f);
        }
    }

    private void PreparePromptVisualStates()
    {
        if (startGamePromptObject != null)
            startGamePromptObject.SetActive(false);

        if (startGameLeftMouseObject != null)
            startGameLeftMouseObject.SetActive(false);

        if (startGameRightMouseObject != null)
            startGameRightMouseObject.SetActive(false);

        if (quitGamePromptObject != null)
            quitGamePromptObject.SetActive(false);

        if (quitGameLeftMouseObject != null)
            quitGameLeftMouseObject.SetActive(false);

        if (quitGameRightMouseObject != null)
            quitGameRightMouseObject.SetActive(false);

        currentPrompt = PromptType.None;
    }

    private void OpenStartGamePrompt()
    {
        if (!carCrash)
        {
            Log("OpenStartGamePrompt called, but carCrash is false.");
            return;
        }

        Log("Opening StartGame prompt.");

        currentPrompt = PromptType.StartGame;
        isHovering = false;
        lastHoverTime = -999f;

        if (startGamePromptObject != null)
            startGamePromptObject.SetActive(true);

        if (startGameLeftMouseObject != null)
            startGameLeftMouseObject.SetActive(true);

        if (startGameRightMouseObject != null)
            startGameRightMouseObject.SetActive(true);

        SetCrosshairsVisible(false);
        StartCooldown();
    }

    private void OpenQuitGamePrompt()
    {
        if (!quitGame)
        {
            Log("OpenQuitGamePrompt called, but quitGame is false.");
            return;
        }

        Log("Opening QuitGame prompt.");

        currentPrompt = PromptType.QuitGame;
        isHovering = false;
        lastHoverTime = -999f;

        if (quitGamePromptObject != null)
            quitGamePromptObject.SetActive(true);

        if (quitGameLeftMouseObject != null)
            quitGameLeftMouseObject.SetActive(true);

        if (quitGameRightMouseObject != null)
            quitGameRightMouseObject.SetActive(true);

        SetCrosshairsVisible(false);
        StartCooldown();
    }

    private void CancelCurrentPrompt()
    {
        Log($"{currentPrompt} prompt canceled.");

        HideAllPrompts();

        SetCrosshairsVisible(true);
        SetCrosshairMode(false);

        isHovering = false;
        lastHoverTime = -999f;

        StartCooldown();
    }

    private void ConfirmCurrentPrompt()
    {
        Log($"{currentPrompt} prompt confirmed.");

        PromptType promptToConfirm = currentPrompt;
        HideAllPrompts();

        SetCrosshairsVisible(false);

        switch (promptToConfirm)
        {
            case PromptType.StartGame:
                if (roadTileManager != null)
                {
                    Log("Calling RoadTileManager.StartCarCrashSequence().");
                    roadTileManager.StartCarCrashSequence();
                }
                else
                {
                    Debug.LogWarning("[MainMenuManager] roadTileManager is NULL, cannot start crash sequence.");
                }
                break;

            case PromptType.QuitGame:
                Log("Quitting application.");
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
                break;
        }

        StartCooldown();
    }

    private void HideAllPrompts()
    {
        if (startGamePromptObject != null)
            startGamePromptObject.SetActive(false);

        if (startGameLeftMouseObject != null)
            startGameLeftMouseObject.SetActive(false);

        if (startGameRightMouseObject != null)
            startGameRightMouseObject.SetActive(false);

        if (quitGamePromptObject != null)
            quitGamePromptObject.SetActive(false);

        if (quitGameLeftMouseObject != null)
            quitGameLeftMouseObject.SetActive(false);

        if (quitGameRightMouseObject != null)
            quitGameRightMouseObject.SetActive(false);

        currentPrompt = PromptType.None;
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
        SetCrosshairMode(false);
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

    private void Log(string message)
    {
        if (enableDebugLogs)
            Debug.Log($"[MainMenuManager] {message}");
    }
}