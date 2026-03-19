using TMPro;
using UnityEngine;

public class GraphicsSettingsManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text qualityPresetText;
    [SerializeField] private TMP_Text textureQualityText;
    [SerializeField] private TMP_Text anisotropicText;
    [SerializeField] private TMP_Text shadowDistanceText;

    [Header("Shadow Distance Presets")]
    [SerializeField] private float[] shadowDistanceOptions = { 50f, 100f, 150f, 200f };

    [Header("LOD Bias Per Quality Preset")]
    [SerializeField] private float[] lodBiasPerPreset = { 0.7f, 1.2f, 2.0f };

    [Header("Auto Detect")]
    [SerializeField] private bool autoDetectOnFirstLaunch = true;

    private const string QualityPresetKey = "Graphics_QualityPreset";
    private const string TextureQualityKey = "Graphics_TextureQuality";
    private const string AnisotropicKey = "Graphics_Anisotropic";
    private const string ShadowDistanceKey = "Graphics_ShadowDistance";
    private const string AutoDetectedKey = "Graphics_AutoDetected";

    // Texture quality:
    // 0 = Full
    // 1 = Half
    // 2 = Quarter
    private readonly string[] textureLabels = { "Full", "Half", "Quarter" };

    // Anisotropic filtering:
    // 0 = Disable
    // 1 = Enable
    // 2 = ForceEnable
    private readonly string[] anisotropicLabels = { "Off", "Per Texture", "Force On" };

    private int currentQualityPreset;
    private int currentTextureQuality;
    private int currentAnisotropic;
    private int currentShadowDistance;

    private void Awake()
    {
        InitializeSettings();
        ApplyAllSettings();
        RefreshAllTexts();
    }

    private void InitializeSettings()
    {
        bool hasSavedSettings =
            PlayerPrefs.HasKey(QualityPresetKey) ||
            PlayerPrefs.HasKey(TextureQualityKey) ||
            PlayerPrefs.HasKey(AnisotropicKey) ||
            PlayerPrefs.HasKey(ShadowDistanceKey);

        if (!hasSavedSettings && autoDetectOnFirstLaunch && PlayerPrefs.GetInt(AutoDetectedKey, 0) == 0)
        {
            AutoDetectAndSaveDefaults();
            PlayerPrefs.SetInt(AutoDetectedKey, 1);
            PlayerPrefs.Save();
        }

        currentQualityPreset = Mathf.Clamp(
            PlayerPrefs.GetInt(QualityPresetKey, GetSafeDefaultQualityPreset()),
            0,
            Mathf.Max(0, QualitySettings.names.Length - 1)
        );

        currentTextureQuality = Mathf.Clamp(
            PlayerPrefs.GetInt(TextureQualityKey, 0),
            0,
            textureLabels.Length - 1
        );

        currentAnisotropic = Mathf.Clamp(
            PlayerPrefs.GetInt(AnisotropicKey, 1),
            0,
            anisotropicLabels.Length - 1
        );

        currentShadowDistance = Mathf.Clamp(
            PlayerPrefs.GetInt(ShadowDistanceKey, 1),
            0,
            Mathf.Max(0, shadowDistanceOptions.Length - 1)
        );
    }

    private void AutoDetectAndSaveDefaults()
    {
        int detectedPreset = DetectQualityPresetFromSpecs();

        PlayerPrefs.SetInt(QualityPresetKey, detectedPreset);

        switch (detectedPreset)
        {
            // Performant
            case 0:
                PlayerPrefs.SetInt(TextureQualityKey, 2);    // Quarter
                PlayerPrefs.SetInt(AnisotropicKey, 0);       // Off
                PlayerPrefs.SetInt(ShadowDistanceKey, 0);    // 50
                break;

            // Balanced
            case 1:
                PlayerPrefs.SetInt(TextureQualityKey, 1);    // Half
                PlayerPrefs.SetInt(AnisotropicKey, 1);       // Per Texture
                PlayerPrefs.SetInt(ShadowDistanceKey, 1);    // 100
                break;

            // High Fidelity
            default:
                PlayerPrefs.SetInt(TextureQualityKey, 0);    // Full
                PlayerPrefs.SetInt(AnisotropicKey, 2);       // Force On
                PlayerPrefs.SetInt(ShadowDistanceKey, 3);    // 200
                break;
        }
    }

    private int DetectQualityPresetFromSpecs()
    {
        int vramMb = SystemInfo.graphicsMemorySize;
        int ramMb = SystemInfo.systemMemorySize;
        int cpuThreads = SystemInfo.processorCount;

        // 0 = Performant
        // 1 = Balanced
        // 2 = High Fidelity

        if (vramMb >= 8192 && ramMb >= 16000 && cpuThreads >= 12)
            return GetClampedPresetIndex(2);

        if (vramMb >= 4096 && ramMb >= 8000 && cpuThreads >= 8)
            return GetClampedPresetIndex(1);

        return GetClampedPresetIndex(0);
    }

    private int GetClampedPresetIndex(int desiredIndex)
    {
        int max = Mathf.Max(0, QualitySettings.names.Length - 1);
        return Mathf.Clamp(desiredIndex, 0, max);
    }

    private int GetSafeDefaultQualityPreset()
    {
        if (QualitySettings.names == null || QualitySettings.names.Length == 0)
            return 0;

        // Prefer middle preset if possible
        if (QualitySettings.names.Length >= 3)
            return 1;

        return 0;
    }

    public void QualityPresetSmaller()
    {
        if (QualitySettings.names == null || QualitySettings.names.Length == 0)
            return;

        currentQualityPreset = Mathf.Max(0, currentQualityPreset - 1);
        ApplyQualityPreset();
        SaveSettings();
        RefreshAllTexts();
    }

    public void QualityPresetBigger()
    {
        if (QualitySettings.names == null || QualitySettings.names.Length == 0)
            return;

        currentQualityPreset = Mathf.Min(QualitySettings.names.Length - 1, currentQualityPreset + 1);
        ApplyQualityPreset();
        SaveSettings();
        RefreshAllTexts();
    }

    public void TextureQualitySmaller()
    {
        currentTextureQuality = Mathf.Max(0, currentTextureQuality - 1);
        ApplyTextureQuality();
        SaveSettings();
        RefreshAllTexts();
    }

    public void TextureQualityBigger()
    {
        currentTextureQuality = Mathf.Min(textureLabels.Length - 1, currentTextureQuality + 1);
        ApplyTextureQuality();
        SaveSettings();
        RefreshAllTexts();
    }

    public void AnisotropicSmaller()
    {
        currentAnisotropic = Mathf.Max(0, currentAnisotropic - 1);
        ApplyAnisotropic();
        SaveSettings();
        RefreshAllTexts();
    }

    public void AnisotropicBigger()
    {
        currentAnisotropic = Mathf.Min(anisotropicLabels.Length - 1, currentAnisotropic + 1);
        ApplyAnisotropic();
        SaveSettings();
        RefreshAllTexts();
    }

    public void ShadowDistanceSmaller()
    {
        currentShadowDistance = Mathf.Max(0, currentShadowDistance - 1);
        ApplyShadowDistance();
        SaveSettings();
        RefreshAllTexts();
    }

    public void ShadowDistanceBigger()
    {
        currentShadowDistance = Mathf.Min(shadowDistanceOptions.Length - 1, currentShadowDistance + 1);
        ApplyShadowDistance();
        SaveSettings();
        RefreshAllTexts();
    }

    private void ApplyAllSettings()
    {
        ApplyQualityPreset();
        ApplyTextureQuality();
        ApplyAnisotropic();
        ApplyShadowDistance();
    }

    private void ApplyQualityPreset()
    {
        if (QualitySettings.names != null && QualitySettings.names.Length > 0)
        {
            currentQualityPreset = Mathf.Clamp(currentQualityPreset, 0, QualitySettings.names.Length - 1);
            QualitySettings.SetQualityLevel(currentQualityPreset, true);
        }

        // Optional extra control over LOD bias per preset
        if (lodBiasPerPreset != null && lodBiasPerPreset.Length > 0)
        {
            int lodIndex = Mathf.Clamp(currentQualityPreset, 0, lodBiasPerPreset.Length - 1);
            QualitySettings.lodBias = lodBiasPerPreset[lodIndex];
        }
    }

    private void ApplyTextureQuality()
    {
        QualitySettings.globalTextureMipmapLimit = currentTextureQuality;
    }

    private void ApplyAnisotropic()
    {
        switch (currentAnisotropic)
        {
            case 0:
                QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
                break;
            case 1:
                QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
                break;
            case 2:
                QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
                break;
        }
    }

    private void ApplyShadowDistance()
    {
        if (shadowDistanceOptions == null || shadowDistanceOptions.Length == 0)
            return;

        currentShadowDistance = Mathf.Clamp(currentShadowDistance, 0, shadowDistanceOptions.Length - 1);
        QualitySettings.shadowDistance = shadowDistanceOptions[currentShadowDistance];
    }

    private void RefreshAllTexts()
    {
        if (qualityPresetText != null)
        {
            if (QualitySettings.names != null && QualitySettings.names.Length > 0)
                qualityPresetText.text = QualitySettings.names[currentQualityPreset];
            else
                qualityPresetText.text = "N/A";
        }

        if (textureQualityText != null)
            textureQualityText.text = textureLabels[currentTextureQuality];

        if (anisotropicText != null)
            anisotropicText.text = anisotropicLabels[currentAnisotropic];

        if (shadowDistanceText != null)
            shadowDistanceText.text = Mathf.RoundToInt(shadowDistanceOptions[currentShadowDistance]).ToString();
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetInt(QualityPresetKey, currentQualityPreset);
        PlayerPrefs.SetInt(TextureQualityKey, currentTextureQuality);
        PlayerPrefs.SetInt(AnisotropicKey, currentAnisotropic);
        PlayerPrefs.SetInt(ShadowDistanceKey, currentShadowDistance);
        PlayerPrefs.Save();
    }

    [ContextMenu("Clear Saved Graphics Settings")]
    public void ClearSavedGraphicsSettings()
    {
        PlayerPrefs.DeleteKey(QualityPresetKey);
        PlayerPrefs.DeleteKey(TextureQualityKey);
        PlayerPrefs.DeleteKey(AnisotropicKey);
        PlayerPrefs.DeleteKey(ShadowDistanceKey);
        PlayerPrefs.DeleteKey(AutoDetectedKey);
        PlayerPrefs.Save();
    }
}