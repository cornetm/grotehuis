using UnityEngine;
using UnityEngine.UI;

public class UIRaycastLayerIgnorer : MonoBehaviour
{
    [Header("Layer that should ignore UI raycasts")]
    [SerializeField] private string ignoreLayerName = "UIIgnoreRaycast";

    [Header("Also include inactive objects")]
    [SerializeField] private bool includeInactive = true;

    private void Awake()
    {
        DisableRaycastTargetsOnLayer();
    }

    [ContextMenu("Disable Raycast Targets On Layer")]
    public void DisableRaycastTargetsOnLayer()
    {
        int targetLayer = LayerMask.NameToLayer(ignoreLayerName);

        if (targetLayer == -1)
        {
            Debug.LogError($"Layer '{ignoreLayerName}' does not exist.");
            return;
        }

        Graphic[] graphics = includeInactive
            ? Resources.FindObjectsOfTypeAll<Graphic>()
            : FindObjectsByType<Graphic>(FindObjectsSortMode.None);

        int changedCount = 0;

        foreach (Graphic graphic in graphics)
        {
            if (graphic == null)
                continue;

            // Skip assets/prefabs not in scene
            if (!graphic.gameObject.scene.IsValid())
                continue;

            if (graphic.gameObject.layer != targetLayer)
                continue;

            if (graphic.raycastTarget)
            {
                graphic.raycastTarget = false;
                changedCount++;
            }
        }

        Debug.Log($"Disabled raycastTarget on {changedCount} UI Graphic(s) in layer '{ignoreLayerName}'.");
    }
}