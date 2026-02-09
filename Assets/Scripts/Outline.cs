using UnityEngine;
using TMPro;

[RequireComponent(typeof(Renderer))]
public class Highlight : MonoBehaviour
{
    private Renderer rend;
    private Material[] originalMaterials;
    private Material[] glowMaterialsInstance;
    private bool highlightActive = false;

    [Header("Glow Outline Settings")]
    public Material outlineGlowMaterial;
    public Color glowColor = Color.white;
    [Range(0.01f, 1f)]
    public float maxAlpha = 0.5f;
    [Range(0f, 5f)]
    public float glowIntensity = 2f;
    [Range(1f, 20f)]
    public float fadeSpeed = 10f;

    [Header("World Text Settings")]
    public bool showItemText = true;      // Toon text bij highlight
    public float textY = 2f;              // Vaste hoogte
    public float textScale = 0.25f;

    private float currentAlpha = 0f;
    private TextMeshPro worldText;
    private Transform cam;

    void Start()
    {
        cam = Camera.main.transform;

        rend = GetComponent<Renderer>();
        originalMaterials = rend.materials;

        glowMaterialsInstance = new Material[originalMaterials.Length];
        for (int i = 0; i < glowMaterialsInstance.Length; i++)
        {
            glowMaterialsInstance[i] = new Material(outlineGlowMaterial);
            SetGlowProperties(glowMaterialsInstance[i], 0.01f);
        }

        CreateWorldText(); // Altijd aanmaken, ongeacht showItemText
    }

    void Update()
    {
        float targetAlpha = highlightActive ? maxAlpha : 0.01f;
        currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, fadeSpeed * Time.deltaTime);

        for (int i = 0; i < glowMaterialsInstance.Length; i++)
            SetGlowProperties(glowMaterialsInstance[i], currentAlpha);

        if (currentAlpha > 0.02f)
        {
            Material[] combined = new Material[originalMaterials.Length + glowMaterialsInstance.Length];
            originalMaterials.CopyTo(combined, 0);
            glowMaterialsInstance.CopyTo(combined, originalMaterials.Length);
            rend.materials = combined;
        }
        else
        {
            rend.materials = originalMaterials;
        }

        UpdateWorldText();
    }

    void CreateWorldText()
    {
        GameObject t = new GameObject("PickupText");
        t.transform.SetParent(transform);

        worldText = t.AddComponent<TextMeshPro>();
        worldText.alignment = TextAlignmentOptions.Center;
        worldText.fontSize = 5;
        worldText.color = Color.white;

        // Altijd uit in begin
        worldText.gameObject.SetActive(false);

        // Zet prefab naam of object naam
        PrefabReferenceAuto pr = GetComponent<PrefabReferenceAuto>();
        if (pr != null && pr.prefab != null)
            worldText.text = pr.prefab.name;
        else
            worldText.text = gameObject.name.Replace("(Clone)", "");
    }

    void UpdateWorldText()
    {
        if (worldText == null) return;

        // Activeer alleen als highlight aan én showItemText true
        worldText.gameObject.SetActive(highlightActive && showItemText);

        // Positioneer boven object
        Vector3 pos = new Vector3(transform.position.x, textY, transform.position.z);
        worldText.transform.position = pos;
        worldText.transform.localScale = Vector3.one * textScale;

        // Draai altijd naar camera
        worldText.transform.rotation = Quaternion.LookRotation(worldText.transform.position - cam.position);
    }

    private void SetGlowProperties(Material mat, float alpha)
    {
        Color col = new Color(glowColor.r, glowColor.g, glowColor.b, alpha);
        mat.SetColor("_BaseColor", col);

        if (mat.HasProperty("_EmissiveColor"))
        {
            mat.SetColor("_EmissiveColor", glowColor * glowIntensity);
            mat.EnableKeyword("_EMISSIVE_COLOR");
        }

        if (mat.HasProperty("_Surface"))
            mat.SetFloat("_Surface", 1f);

        if (mat.HasProperty("_ZWrite"))
            mat.SetFloat("_ZWrite", 0);

        mat.renderQueue = 3100;
    }

    public void EnableHighlight() => highlightActive = true;
    public void DisableHighlight() => highlightActive = false;
}
