using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class SprintStressEffectHDRP : MonoBehaviour
{
    public PlayerMovement player;
    public Volume volume; // HDRP Volume

    private Vignette vignette;
    private ChromaticAberration chromatic;

    [Header("Base values")]
    public float baseVignette = 0.18f;
    public float sprintVignetteMultiplier = 2f; // verdubbelen bij sprint
    public float baseChromatic = 0.04f;
    public float sprintChromaticMultiplier = 2f;

    [Header("Smooth speed")]
    public float lerpSpeed = 5f;

    void Start()
    {
        if (volume.profile.TryGet<Vignette>(out var v))
            vignette = v;
        if (volume.profile.TryGet<ChromaticAberration>(out var c))
            chromatic = c;
    }

    void Update()
    {
        if (player == null) return;

        // Target waarden afhankelijk van sprint
        float targetVignette = baseVignette;
        float targetChromatic = baseChromatic;

        if (player.isSprinting)
        {
            targetVignette *= sprintVignetteMultiplier;
            targetChromatic *= sprintChromaticMultiplier;
        }

        // Smooth toepassen
        if (vignette != null)
            vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, targetVignette, Time.deltaTime * lerpSpeed);

        if (chromatic != null)
            chromatic.intensity.value = Mathf.Lerp(chromatic.intensity.value, targetChromatic, Time.deltaTime * lerpSpeed);
    }
}
