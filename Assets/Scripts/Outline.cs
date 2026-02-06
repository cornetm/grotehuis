using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class Highlight : MonoBehaviour
{
    private Renderer rend;
    private Material[] originalMaterials;
    private Material[] glowMaterialsInstance;
    private bool highlightActive = false;

    [Header("Glow Outline Settings")]
    public Material outlineGlowMaterial;  // HDRP Lit outline shader met emission
    public Color glowColor = Color.white; // Kleur van de glow
    [Range(0.01f, 1f)]
    public float maxAlpha = 0.5f;         // Max transparantie (niet 0!)
    [Range(0f, 5f)]
    public float glowIntensity = 2f;      // Sterkte emissive gloed
    [Range(1f, 20f)]
    public float fadeSpeed = 10f;         // Hoe snel glow fade-in/out

    private float currentAlpha = 0f;

    void Start()
    {
        rend = GetComponent<Renderer>();
        originalMaterials = rend.materials;

        // Maak per originele materiaal een glow instance
        glowMaterialsInstance = new Material[originalMaterials.Length];
        for (int i = 0; i < glowMaterialsInstance.Length; i++)
        {
            glowMaterialsInstance[i] = new Material(outlineGlowMaterial);
            SetGlowProperties(glowMaterialsInstance[i], 0.01f); // start bijna transparant
        }
    }

    void Update()
    {
        // Smooth fade-in/fade-out
        float targetAlpha = highlightActive ? maxAlpha : 0.01f;
        currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, fadeSpeed * Time.deltaTime);

        // Update glow-material properties
        for (int i = 0; i < glowMaterialsInstance.Length; i++)
            SetGlowProperties(glowMaterialsInstance[i], currentAlpha);

        // Combineer originele materialen + glow overlay
        if (currentAlpha > 0.01f)
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
    }

    private void SetGlowProperties(Material mat, float alpha)
    {
        // BaseColor met alpha (transparant)
        Color col = new Color(glowColor.r, glowColor.g, glowColor.b, alpha);
        mat.SetColor("_BaseColor", col);

        // Emissive
        if (mat.HasProperty("_EmissiveColor"))
        {
            mat.SetColor("_EmissiveColor", glowColor * glowIntensity);
            mat.EnableKeyword("_EMISSIVE_COLOR");
        }

        // Transparent surface
        if (mat.HasProperty("_Surface"))
            mat.SetFloat("_Surface", 1f); // Transparent

        // Depth Write uit
        if (mat.HasProperty("_ZWrite"))
            mat.SetFloat("_ZWrite", 0);

        // Render queue hoger dan origineel
        mat.renderQueue = 3100;
    }

    public void EnableHighlight() => highlightActive = true;
    public void DisableHighlight() => highlightActive = false;
}
