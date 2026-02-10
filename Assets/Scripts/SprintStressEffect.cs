using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class SprintStressEffectHDRP : MonoBehaviour
{
    public PlayerMovement player;
    public Volume volume; // HDRP Volume
    private Vignette vignette;
    private ChromaticAberration chromatic;

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

        // Subtiele sprint stress
        float targetChromatic = player.isSprinting ? 0.04f : 0f;
        if (chromatic != null)
            chromatic.intensity.value = Mathf.Lerp(chromatic.intensity.value, targetChromatic, Time.deltaTime * 5f);

        // Optioneel: vignette intensiteit verhogen bij sprint
        float targetVignette = player.isSprinting ? 0.25f : 0.18f;
        if (vignette != null)
            vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, targetVignette, Time.deltaTime * 5f);
    }
}
