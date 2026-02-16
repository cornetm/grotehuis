using UnityEngine;

public class StaticObjectSpawner : MonoBehaviour
{
    [Header("Objects to Spawn")]
    public GameObject[] objectPrefabs; // tafels, stoelen, piano's, kasten
    public int spawnCount = 5;

    [Header("Spawn Area")]
    public Vector3 spawnArea = new Vector3(10f, 0f, 10f);
    public float yOffset = 0f;

    [Header("Placement Settings")]
    public int maxAttempts = 50;      // aantal pogingen per object
    public float clearance = 0.1f;    // extra afstand tussen objecten

    void Start()
    {
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

                // Kies een random prefab
                GameObject prefab = objectPrefabs[Random.Range(0, objectPrefabs.Length)];

                // Bereken random positie
                Vector3 pos = transform.position + new Vector3(
                    Random.Range(-spawnArea.x * 0.5f, spawnArea.x * 0.5f),
                    yOffset,
                    Random.Range(-spawnArea.z * 0.5f, spawnArea.z * 0.5f)
                );

                // Bepaal prefab bounds
                Collider prefabCollider = prefab.GetComponent<Collider>();
                if (prefabCollider == null)
                {
                    Debug.LogWarning(prefab.name + " heeft geen collider!");
                    continue;
                }

                Vector3 halfExtents = prefabCollider.bounds.extents + Vector3.one * clearance;

                // Check overlap met Physics
                Collider[] overlaps = Physics.OverlapBox(pos, halfExtents, prefab.transform.rotation);
                if (overlaps.Length == 0)
                {
                    // Plaats object
                    GameObject spawnedObj = Instantiate(prefab, pos, prefab.transform.rotation);
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

    // ================= GIZMO =================
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.25f); // cyaan transparant
        Vector3 center = transform.position + new Vector3(0, yOffset, 0);
        Vector3 size = new Vector3(spawnArea.x, 0.1f, spawnArea.z);
        Gizmos.DrawCube(center, size);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(center, size);
    }
}
