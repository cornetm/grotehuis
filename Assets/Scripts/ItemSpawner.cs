using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("Items to Spawn")]
    public GameObject[] itemPrefabs;        // Prefabs die je wilt spawnen
    public Texture[] itemIcons;             // Optioneel: icons voor inventory (zelfde index als prefab)

    [Header("Spawn Settings")]
    public int spawnCount = 5;              // Hoeveel items automatisch spawnen bij start
    public Vector3 spawnArea = new Vector3(5f, 0f, 5f); // Grootte van spawn gebied
    public float yOffset = 0.5f;            // Hoogte waarop items verschijnen
    public bool randomRotation = true;      // Willekeurige rotatie

    void Start()
    {
        SpawnInitialItems();
    }

    // ================= SPAWN INIT =================
    private void SpawnInitialItems()
    {
        if (itemPrefabs.Length == 0)
        {
            Debug.LogWarning("Geen prefabs toegewezen aan ItemSpawner!");
            return;
        }

        for (int i = 0; i < spawnCount; i++)
        {
            SpawnRandomItem();
        }
    }

    // ================= SPAWN RANDOM ITEM =================
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

        // Voeg PrefabReferenceAuto toe als het niet bestaat
        PrefabReferenceAuto prefabRef = spawned.GetComponent<PrefabReferenceAuto>();
        if (prefabRef == null)
            prefabRef = spawned.AddComponent<PrefabReferenceAuto>();

        prefabRef.prefab = prefab;
        if (itemIcons.Length > index)
            prefabRef.icon = itemIcons[index];

        if (!spawned.CompareTag("Interaction"))
            spawned.tag = "Interaction";
    }

    // ================= SPAWN DROPPED ITEM =================
    public void SpawnDroppedItem(GameObject prefab, Vector3 position)
    {
        if (prefab == null) return;

        Quaternion rot = randomRotation ? Quaternion.Euler(0f, Random.Range(0f, 360f), 0f) : Quaternion.identity;
        GameObject spawned = Instantiate(prefab, position, rot);

        PrefabReferenceAuto prefabRef = spawned.GetComponent<PrefabReferenceAuto>();
        if (prefabRef == null)
            prefabRef = spawned.AddComponent<PrefabReferenceAuto>();

        prefabRef.prefab = prefab;
        prefabRef.icon = null;

        if (!spawned.CompareTag("Interaction"))
            spawned.tag = "Interaction";
    }

    // ================= GIZMOS =================
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawCube(transform.position + new Vector3(0, yOffset, 0), spawnArea + Vector3.up * 0.1f);
    }
}
