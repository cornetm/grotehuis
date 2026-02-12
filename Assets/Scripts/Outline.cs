using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class Highlight : MonoBehaviour
{
    [Header("Glow Settings")]
    public Color glowColor = Color.white;
    public float glowIntensity = 2f;
    public float fadeSpeed = 10f;

    private bool highlightActive = false;
    private float currentAlpha = 0f;

    private Renderer[] renderers;
    private Dictionary<Renderer, Material[]> originalMaterials = new();
    private Dictionary<Renderer, Material[]> glowMaterials = new();

    private TextMeshPro worldText;
    public float textY = 2f;
    public float textScale = 0.25f;
    public bool showItemText = true;
    private Transform cam;

    void Start()
    {
        cam = Camera.main.transform;

        renderers = GetComponentsInChildren<Renderer>();
        foreach (var rend in renderers)
        {
            originalMaterials[rend] = rend.materials;
            Material[] glowMats = new Material[rend.materials.Length];

            for (int i = 0; i < glowMats.Length; i++)
            {
                // Alleen emissive copy maken
                Material mat = new Material(rend.materials[i]);
                if (mat.HasProperty("_EmissiveColor"))
                {
                    mat.SetColor("_EmissiveColor", glowColor * 0.01f);
                    mat.EnableKeyword("_EMISSIVE_COLOR");
                }
                glowMats[i] = mat;
            }

            glowMaterials[rend] = glowMats;
        }

        CreateWorldText();
    }

    void Update()
    {
        float targetAlpha = highlightActive ? 1f : 0.01f;
        currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, fadeSpeed * Time.deltaTime);

        foreach (var rend in renderers)
        {
            Material[] mats = glowMaterials[rend];
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i].HasProperty("_EmissiveColor"))
                    mats[i].SetColor("_EmissiveColor", glowColor * glowIntensity * currentAlpha);
            }

            if (currentAlpha > 0.02f)
                rend.materials = mats;
            else
                rend.materials = originalMaterials[rend];
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
        worldText.gameObject.SetActive(false);

        worldText.text = gameObject.name.Replace("(Clone)", "");
    }

    void UpdateWorldText()
    {
        if (worldText == null) return;
        worldText.gameObject.SetActive(highlightActive && showItemText);
        worldText.transform.position = new Vector3(transform.position.x, textY, transform.position.z);
        worldText.transform.localScale = Vector3.one * textScale;
        worldText.transform.rotation = Quaternion.LookRotation(worldText.transform.position - cam.position);
    }

    public void EnableHighlight() => highlightActive = true;
    public void DisableHighlight() => highlightActive = false;
}
