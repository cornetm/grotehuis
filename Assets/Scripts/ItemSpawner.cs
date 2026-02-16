using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("Items to Spawn")]
    public GameObject[] itemPrefabs;
    public Texture[] itemIcons;

    [Header("Spawn Settings")]
    public int spawnCount = 5;
    public Vector3 spawnArea = new Vector3(5f, 0f, 5f);
    public float yOffset = 0.5f;
    public bool randomRotation = true;

    void Start() => SpawnInitialItems();

    private void SpawnInitialItems()
    {
        if (itemPrefabs.Length == 0) { Debug.LogWarning("No prefabs assigned!"); return; }
        for (int i = 0; i < spawnCount; i++) SpawnRandomItem();
    }

    private void SpawnRandomItem()
    {
        int index = Random.Range(0, itemPrefabs.Length);
        GameObject prefab = itemPrefabs[index];

        Vector3 pos = transform.position + new Vector3(
            Random.Range(-spawnArea.x * 0.5f, spawnArea.x * 0.5f),
            yOffset,
            Random.Range(-spawnArea.z * 0.5f, spawnArea.z * 0.5f)
        );

        // Full random rotation
        Quaternion rot = randomRotation
            ? Quaternion.Euler(
                Random.Range(0f, 360f),
                Random.Range(0f, 360f),
                Random.Range(0f, 360f)
              )
            : Quaternion.identity;

        GameObject spawned = Instantiate(prefab, pos, rot);

        PrefabReferenceAuto prefabRef = spawned.GetComponent<PrefabReferenceAuto>();
        if (prefabRef == null) prefabRef = spawned.AddComponent<PrefabReferenceAuto>();

        prefabRef.prefab = prefab;
        if (itemIcons.Length > index) prefabRef.icon = itemIcons[index];

        if (!spawned.CompareTag("Interaction"))
            spawned.tag = "Interaction";
    }

    // ================= SPAWN DROPPED ITEM =================
    public GameObject SpawnDroppedItem(GameObject prefab, Vector3 position, bool randomRotationForDrop = true)
    {
        if (prefab == null) return null;

        Quaternion rotation = randomRotationForDrop
            ? Quaternion.Euler(
                Random.Range(0f, 360f),
                Random.Range(0f, 360f),
                Random.Range(0f, 360f)
              )
            : Quaternion.identity;

        GameObject spawned = Instantiate(prefab, position, rotation);

        PrefabReferenceAuto prefabRef = spawned.GetComponent<PrefabReferenceAuto>();
        if (prefabRef == null)
            prefabRef = spawned.AddComponent<PrefabReferenceAuto>();

        prefabRef.prefab = prefab;
        prefabRef.icon = null;

        if (!spawned.CompareTag("Interaction"))
            spawned.tag = "Interaction";

        return spawned;
    }

    // ================= GIZMO =================
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 1f, 0f, 0.25f); // geel transparant
        Vector3 center = transform.position + new Vector3(0, yOffset, 0);
        Vector3 size = new Vector3(spawnArea.x, 0.1f, spawnArea.z); // dunne hoogte
        Gizmos.DrawCube(center, size);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(center, size);
    }
}
