using UnityEngine;

[ExecuteAlways]
public class EditorLightToggle : MonoBehaviour
{
    [SerializeField] private bool isOn = true;
    [SerializeField] private Light targetLight;
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Material onMaterial;
    [SerializeField] private Material offMaterial;

    private void OnValidate()
    {
        ApplyState();
    }

    private void ApplyState()
    {
        if (targetLight != null)
        {
            targetLight.enabled = isOn;
        }

        if (targetRenderer != null)
        {
            if (isOn && onMaterial != null)
            {
                targetRenderer.sharedMaterial = onMaterial;
            }
            else if (!isOn && offMaterial != null)
            {
                targetRenderer.sharedMaterial = offMaterial;
            }
        }
    }
}