using UnityEngine;

public class PrefabReferenceAuto : MonoBehaviour
{
    [Header("Prefab Reference (Auto)")]
    public GameObject prefab; // Automatisch ingesteld
    public Texture icon;      // Optioneel icon voor inventory

    private void Awake()
    {
        if (prefab == null)
        {
            // Zoek prefab in Resources/Items/ folder op basis van naam (zonder "(Clone)")
            string prefabName = gameObject.name.Replace("(Clone)", "");
            GameObject foundPrefab = Resources.Load<GameObject>("Items/" + prefabName);
            if (foundPrefab != null)
            {
                prefab = foundPrefab;
            }
            else
            {
                Debug.LogWarning($"PrefabReferenceAuto kon geen prefab vinden voor {prefabName}");
            }
        }
    }
}
