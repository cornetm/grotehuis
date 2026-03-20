using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("Items to Spawn")]
    public GameObject[] itemPrefabs;
    public Texture[] itemIcons;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    [Header("Spawn Settings")]
    public float yOffset = 0.5f;
    public bool randomRotation = true;

    private Transform itemsParent;

    // 🔹 Max 2 items per spawnpoint
    private int[] spawnCounts;
    private const int maxPerSpawnPoint = 2;

    void Start()
    {
        SetupParent();

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("No spawn points assigned!");
            return;
        }

        spawnCounts = new int[spawnPoints.Length];

        SpawnAllItems();
    }

    private void SetupParent()
    {
        GameObject parentObj = GameObject.Find("Items");

        if (parentObj == null)
            parentObj = new GameObject("Items");

        itemsParent = parentObj.transform;
    }

    // 🔹 vult ALLE spawnpoints (max 2 per point)
    private void SpawnAllItems()
    {
        if (itemPrefabs == null || itemPrefabs.Length == 0)
        {
            Debug.LogWarning("No prefabs assigned!");
            return;
        }

        bool spawnedSomething = true;

        // 🔹 blijft proberen tot alles vol zit
        while (spawnedSomething)
        {
            spawnedSomething = SpawnRandomItem();
        }
    }

    private bool SpawnRandomItem()
    {
        int spawnIndex = -1;
        int attempts = 0;

        // 🔹 zoek spawnpoint met ruimte
        while (attempts < 20)
        {
            int randomIndex = Random.Range(0, spawnPoints.Length);

            if (spawnCounts[randomIndex] < maxPerSpawnPoint)
            {
                spawnIndex = randomIndex;
                break;
            }

            attempts++;
        }

        if (spawnIndex == -1)
            return false; // alles vol

        spawnCounts[spawnIndex]++;

        Transform spawnPoint = spawnPoints[spawnIndex];

        int prefabIndex = Random.Range(0, itemPrefabs.Length);
        GameObject prefab = itemPrefabs[prefabIndex];

        Vector3 offset = new Vector3(
            Random.Range(-0.3f, 0.3f),
            0,
            Random.Range(-0.3f, 0.3f)
        );

        Vector3 pos = spawnPoint.position + new Vector3(0, yOffset, 0) + offset;

        Quaternion rot = randomRotation
            ? Quaternion.Euler(
                Random.Range(0f, 360f),
                Random.Range(0f, 360f),
                Random.Range(0f, 360f)
              )
            : spawnPoint.rotation;

        GameObject spawned = Instantiate(prefab, pos, rot, itemsParent);

        PrefabReferenceAuto prefabRef = spawned.GetComponent<PrefabReferenceAuto>();
        if (prefabRef == null)
            prefabRef = spawned.AddComponent<PrefabReferenceAuto>();

        prefabRef.prefab = prefab;

        if (itemIcons.Length > prefabIndex)
            prefabRef.icon = itemIcons[prefabIndex];

        if (!spawned.CompareTag("Interaction"))
            spawned.tag = "Interaction";

        return true;
    }

    // ================= DROPPED ITEM =================
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

        GameObject spawned = Instantiate(prefab, position, rotation, itemsParent);

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