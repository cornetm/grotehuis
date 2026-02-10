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

        Quaternion rot = randomRotation ? Quaternion.Euler(0f, Random.Range(0f, 360f), 0f) : Quaternion.identity;
        GameObject spawned = Instantiate(prefab, pos, rot);

        PrefabReferenceAuto prefabRef = spawned.GetComponent<PrefabReferenceAuto>();
        if (prefabRef == null) prefabRef = spawned.AddComponent<PrefabReferenceAuto>();

        prefabRef.prefab = prefab;
        if (itemIcons.Length > index) prefabRef.icon = itemIcons[index];

        if (!spawned.CompareTag("Interaction"))
            spawned.tag = "Interaction";
    }

    // ================= SPAWN DROPPED ITEM =================
    public GameObject SpawnDroppedItem(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return null;

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
}
