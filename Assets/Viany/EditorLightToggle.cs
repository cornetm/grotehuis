using UnityEngine;

[ExecuteAlways]
public class EditorLightToggle : MonoBehaviour
{
    [SerializeField] private bool isOn = true;

    [SerializeField] private bool useMultipleLights;

    [SerializeField] private Light singleLight;
    [SerializeField] private Light[] multipleLights;

    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Material onMaterial;
    [SerializeField] private Material offMaterial;

    private void OnValidate()
    {
        ApplyState();
    }

    private void ApplyState()
    {
        if (useMultipleLights)
        {
            for (int i = 0; i < multipleLights.Length; i++)
            {
                multipleLights[i].enabled = isOn;
            }
        }
        else
        {
            singleLight.enabled = isOn;
        }

        targetRenderer.sharedMaterial = isOn ? onMaterial : offMaterial;
    }
}