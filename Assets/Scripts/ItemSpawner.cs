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

    public enum SpawnType
    {
        World,
        Drawer
    }

    [Header("Spawn Mode")]
    public SpawnType spawnType;

    // 🔥 NIEUW
    [Header("Drawer Settings")]
    public Transform drawerTransform;

    [Header("Spawn Chances (%)")]
    [Range(0, 100)] public float chanceOneItem = 60f;
    [Range(0, 100)] public float chanceTwoItems = 25f;

    private Transform itemsParent;

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

    private void SpawnAllItems()
    {
        if (itemPrefabs == null || itemPrefabs.Length == 0)
        {
            Debug.LogWarning("No prefabs assigned!");
            return;
        }

        bool spawnedSomething = true;

        while (spawnedSomething)
        {
            spawnedSomething = SpawnRandomItem();
        }
    }

    private bool SpawnRandomItem()
    {
        int spawnIndex = -1;
        int attempts = 0;

        while (attempts < 20)
        {
            int randomIndex = Random.Range(0, spawnPoints.Length);

            if (spawnCounts[randomIndex] == 0)
            {
                spawnIndex = randomIndex;
                break;
            }

            attempts++;
        }

        if (spawnIndex == -1)
            return false;

        Transform spawnPoint = spawnPoints[spawnIndex];

        float roll = Random.Range(0f, 100f);

        int itemsToSpawn = 0;

        if (roll <= chanceTwoItems)
            itemsToSpawn = 2;
        else if (roll <= chanceTwoItems + chanceOneItem)
            itemsToSpawn = 1;
        else
            itemsToSpawn = 0;

        if (itemsToSpawn == 0)
        {
            spawnCounts[spawnIndex] = maxPerSpawnPoint;
            return true;
        }

        for (int i = 0; i < itemsToSpawn; i++)
        {
            spawnCounts[spawnIndex]++;

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

            // 🔥 HIER IS DE FIX
            Transform parentToUse = itemsParent;

            if (spawnType == SpawnType.Drawer)
            {
                parentToUse = drawerTransform != null ? drawerTransform : spawnPoint;
            }

            GameObject spawned = Instantiate(prefab, pos, rot, parentToUse);

            if (spawnType == SpawnType.Drawer)
            {
                Rigidbody rb = spawned.GetComponent<Rigidbody>();
                if (rb != null)
                    rb.isKinematic = true;
            }

            PrefabReferenceAuto prefabRef = spawned.GetComponent<PrefabReferenceAuto>();
            if (prefabRef == null)
                prefabRef = spawned.AddComponent<PrefabReferenceAuto>();

            prefabRef.prefab = prefab;

            if (itemIcons.Length > prefabIndex)
                prefabRef.icon = itemIcons[prefabIndex];

            if (!spawned.CompareTag("Interaction"))
                spawned.tag = "Interaction";
        }

        return true;
    }

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

        PrefabReferenceAuto prefabData = prefab.GetComponent<PrefabReferenceAuto>();
        if (prefabData != null)
            prefabRef.icon = prefabData.icon;

        if (!spawned.CompareTag("Interaction"))
            spawned.tag = "Interaction";

        return spawned;
    }
}