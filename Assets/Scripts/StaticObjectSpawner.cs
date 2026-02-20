using UnityEngine;

public class StaticObjectSpawner : MonoBehaviour
{
    [Header("Objects to Spawn")]
    public GameObject[] objectPrefabs;
    public int spawnCount = 5;

    [Header("Spawn Area")]
    public Vector3 spawnArea = new Vector3(10f, 0f, 10f);
    public float yOffset = 0f;

    [Header("Placement Settings")]
    public int maxAttempts = 50;
    public float clearance = 0.1f;

    private Transform objectsParent;

    void Start()
    {
        // 🔹 Zoek of maak Objects parent
        GameObject parentObj = GameObject.Find("Objects");

        if (parentObj == null)
        {
            parentObj = new GameObject("Objects");
        }

        objectsParent = parentObj.transform;

        SpawnObjects();
    }

    void SpawnObjects()
    {
        if (objectPrefabs.Length == 0)
        {
            Debug.LogWarning("No object prefabs assigned!");
            return;
        }

        int spawned = 0;

        while (spawned < spawnCount)
        {
            bool placed = false;
            int attempts = 0;

            while (!placed && attempts < maxAttempts)
            {
                attempts++;

                GameObject prefab = objectPrefabs[Random.Range(0, objectPrefabs.Length)];

                Vector3 pos = transform.position + new Vector3(
                    Random.Range(-spawnArea.x * 0.5f, spawnArea.x * 0.5f),
                    yOffset,
                    Random.Range(-spawnArea.z * 0.5f, spawnArea.z * 0.5f)
                );

                Collider prefabCollider = prefab.GetComponent<Collider>();
                if (prefabCollider == null)
                {
                    Debug.LogWarning(prefab.name + " heeft geen collider!");
                    continue;
                }

                Vector3 halfExtents = prefabCollider.bounds.extents + Vector3.one * clearance;

                Collider[] overlaps = Physics.OverlapBox(pos, halfExtents, prefab.transform.rotation);

                if (overlaps.Length == 0)
                {
                    GameObject spawnedObj = Instantiate(prefab, pos, prefab.transform.rotation);

                    // 🔹 FORCE parent to Objects
                    spawnedObj.transform.SetParent(objectsParent);

                    placed = true;
                    spawned++;
                }
            }

            if (!placed)
            {
                Debug.LogWarning("Kon object niet plaatsen na max pogingen!");
                break;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.25f);
        Vector3 center = transform.position + new Vector3(0, yOffset, 0);
        Vector3 size = new Vector3(spawnArea.x, 0.1f, spawnArea.z);
        Gizmos.DrawCube(center, size);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(center, size);
    }
}
