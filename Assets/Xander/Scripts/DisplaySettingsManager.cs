using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DisplaySettingsManager : MonoBehaviour
{
    [System.Serializable]
    public struct ResolutionOption
    {
        public int width;
        public int height;

        public ResolutionOption(int width, int height)
        {
            this.width = width;
            this.height = height;
        }

        public override string ToString()
        {
            return width + " x " + height;
        }
    }

    [Header("UI References")]
    [SerializeField] private TMP_Text resolutionText;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Toggle vSyncToggle;

    [Header("FPS UI")]
    [SerializeField] private Toggle showFpsToggle;
    [SerializeField] private TMP_Text fpsText;

    [Header("Resolution Presets")]
    [SerializeField]
    private ResolutionOption[] resolutions =
    {
        new ResolutionOption(1280, 720),
        new ResolutionOption(1600, 900),
        new ResolutionOption(1920, 1080),
        new ResolutionOption(2560, 1440),
        new ResolutionOption(3840, 2160)
    };

    [Header("Defaults")]
    [SerializeField] private bool defaultFullscreen = true;
    [SerializeField] private bool defaultVSync = false;
    [SerializeField] private bool defaultShowFps = false;

    private int currentResolutionIndex;

    private const string ResolutionIndexKey = "ResolutionIndex";
    private const string FullscreenKey = "Fullscreen";
    private const string VSyncKey = "VSync";
    private const string ShowFpsKey = "ShowFPS";

    private bool isInitializing;

    private float fpsTimer;
    private int fpsFrameCount;

    private void Awake()
    {
        isInitializing = true;

        LoadSettings();
        ApplyAllSettings();
        UpdateResolutionText();
        UpdateToggleVisuals();
        UpdateFpsVisibility();

        if (fullscreenToggle != null)
        {
            fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenToggleChanged);
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggleChanged);
        }

        if (vSyncToggle != null)
        {
            vSyncToggle.onValueChanged.RemoveListener(OnVSyncToggleChanged);
            vSyncToggle.onValueChanged.AddListener(OnVSyncToggleChanged);
        }

        if (showFpsToggle != null)
        {
            showFpsToggle.onValueChanged.RemoveListener(OnShowFpsToggleChanged);
            showFpsToggle.onValueChanged.AddListener(OnShowFpsToggleChanged);
        }

        isInitializing = false;
    }

    private void Start()
    {
        if (fpsText != null)
            fpsText.text = "";
    }

    private void Update()
    {
        if (fpsText == null || !GetSavedShowFps())
            return;

        fpsFrameCount++;
        fpsTimer += Time.unscaledDeltaTime;

        if (fpsTimer >= 0.25f)
        {
            float fps = fpsFrameCount / fpsTimer;
            fpsText.text = Mathf.RoundToInt(fps) + " FPS";

            fpsFrameCount = 0;
            fpsTimer = 0f;
        }
    }

    public void ResolutionSmaller()
    {
        if (resolutions == null || resolutions.Length == 0)
            return;

        currentResolutionIndex = Mathf.Max(0, currentResolutionIndex - 1);

        ApplyResolution();
        UpdateResolutionText();
        SaveSettings();
    }

    public void ResolutionBigger()
    {
        if (resolutions == null || resolutions.Length == 0)
            return;

        currentResolutionIndex = Mathf.Min(resolutions.Length - 1, currentResolutionIndex + 1);

        ApplyResolution();
        UpdateResolutionText();
        SaveSettings();
    }

    private void OnFullscreenToggleChanged(bool value)
    {
        if (isInitializing)
            return;

        ApplyFullscreen(value);
        SaveSettings();
    }

    private void OnVSyncToggleChanged(bool value)
    {
        if (isInitializing)
            return;

        QualitySettings.vSyncCount = value ? 1 : 0;
        SaveSettings();
    }

    private void OnShowFpsToggleChanged(bool value)
    {
        if (isInitializing)
            return;

        PlayerPrefs.SetInt(ShowFpsKey, value ? 1 : 0);
        PlayerPrefs.Save();

        UpdateFpsVisibility();
    }

    private void ApplyAllSettings()
    {
        bool fullscreen = PlayerPrefs.GetInt(FullscreenKey, defaultFullscreen ? 1 : 0) == 1;
        bool vSync = PlayerPrefs.GetInt(VSyncKey, defaultVSync ? 1 : 0) == 1;

        ApplyFullscreen(fullscreen);
        QualitySettings.vSyncCount = vSync ? 1 : 0;
    }

    private void ApplyResolution()
    {
        if (resolutions == null || resolutions.Length == 0)
            return;

        ResolutionOption selected = resolutions[currentResolutionIndex];
        FullScreenMode mode = GetSavedFullscreen() ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;

        Screen.SetResolution(selected.width, selected.height, mode);
    }

    private void ApplyFullscreen(bool fullscreen)
    {
        ResolutionOption selected = resolutions[currentResolutionIndex];
        FullScreenMode mode = fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;

        PlayerPrefs.SetInt(FullscreenKey, fullscreen ? 1 : 0);

        Screen.fullScreenMode = mode;
        Screen.SetResolution(selected.width, selected.height, mode);
    }

    private void UpdateResolutionText()
    {
        if (resolutionText == null || resolutions == null || resolutions.Length == 0)
            return;

        resolutionText.text = resolutions[currentResolutionIndex].ToString();
    }

    private void UpdateToggleVisuals()
    {
        bool fullscreen = GetSavedFullscreen();
        bool vSync = PlayerPrefs.GetInt(VSyncKey, defaultVSync ? 1 : 0) == 1;
        bool showFps = GetSavedShowFps();

        if (fullscreenToggle != null)
            fullscreenToggle.SetIsOnWithoutNotify(fullscreen);

        if (vSyncToggle != null)
            vSyncToggle.SetIsOnWithoutNotify(vSync);

        if (showFpsToggle != null)
            showFpsToggle.SetIsOnWithoutNotify(showFps);
    }

    private void UpdateFpsVisibility()
    {
        bool showFps = GetSavedShowFps();

        if (fpsText != null)
        {
            fpsText.gameObject.SetActive(showFps);

            if (!showFps)
                fpsText.text = "";
        }

        fpsTimer = 0f;
        fpsFrameCount = 0;
    }

    private void LoadSettings()
    {
        if (resolutions == null || resolutions.Length == 0)
            return;

        currentResolutionIndex = PlayerPrefs.GetInt(ResolutionIndexKey, GetClosestResolutionIndex());
        currentResolutionIndex = Mathf.Clamp(currentResolutionIndex, 0, resolutions.Length - 1);
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetInt(ResolutionIndexKey, currentResolutionIndex);
        PlayerPrefs.SetInt(VSyncKey, QualitySettings.vSyncCount > 0 ? 1 : 0);
        PlayerPrefs.Save();
    }

    private bool GetSavedFullscreen()
    {
        return PlayerPrefs.GetInt(FullscreenKey, defaultFullscreen ? 1 : 0) == 1;
    }

    private bool GetSavedShowFps()
    {
        return PlayerPrefs.GetInt(ShowFpsKey, defaultShowFps ? 1 : 0) == 1;
    }

    private int GetClosestResolutionIndex()
    {
        int screenWidth = Screen.currentResolution.width;
        int screenHeight = Screen.currentResolution.height;

        int bestIndex = 0;
        int bestScore = int.MaxValue;

        for (int i = 0; i < resolutions.Length; i++)
        {
            int score = Mathf.Abs(resolutions[i].width - screenWidth) +
                        Mathf.Abs(resolutions[i].height - screenHeight);

            if (score < bestScore)
            {
                bestScore = score;
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    [ContextMenu("Clear Saved Display Settings")]
    public void ClearSavedDisplaySettings()
    {
        PlayerPrefs.DeleteKey(ResolutionIndexKey);
        PlayerPrefs.DeleteKey(FullscreenKey);
        PlayerPrefs.DeleteKey(VSyncKey);
        PlayerPrefs.DeleteKey(ShowFpsKey);
        PlayerPrefs.Save();
    }
}