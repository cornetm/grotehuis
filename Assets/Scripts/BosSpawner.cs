using System.Collections.Generic;
using UnityEngine;

public class BosSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject[] boomPrefabs;

    [Header("Spawn Settings")]
    public int minBomen = 10;
    public int maxBomen = 25;

    [Header("Afstand regels")]
    public float minAfstand = 3f;
    public int maxPogingenPerBoom = 20;

    [Header("Spawn Area (Box)")]
    public Vector3 areaSize = new Vector3(20f, 0f, 20f);

    [Header("Rand instellingen")]
    public int bomenPerRand = 5;

    [Header("Optional")]
    public bool spawnOnStart = true;
    public bool randomRotation = true;

    private static List<Vector3> allSpawnedPositions = new List<Vector3>();

    void Start()
    {
        if (spawnOnStart)
        {
            SpawnBomen();
        }
    }

    public void SpawnBomen()
    {
        if (boomPrefabs == null || boomPrefabs.Length == 0)
        {
            Debug.LogWarning("Geen boom prefabs ingesteld!");
            return;
        }

        // 🔥 Eerst randen vullen
        SpawnRanden();

        int amount = Random.Range(minBomen, maxBomen + 1);

        for (int i = 0; i < amount; i++)
        {
            bool placed = false;

            for (int attempt = 0; attempt < maxPogingenPerBoom; attempt++)
            {
                Vector3 randomPos = new Vector3(
                    Random.Range(-areaSize.x / 2f, areaSize.x / 2f),
                    0f,
                    Random.Range(-areaSize.z / 2f, areaSize.z / 2f)
                );

                Vector3 spawnPos = transform.position + randomPos;

                if (IsValidPosition(spawnPos))
                {
                    SpawnBoom(spawnPos);
                    placed = true;
                    break;
                }
            }

            // 🔴 NIEUW: als het niet lukt → STOP hele spawn proces
            if (!placed)
            {
                Debug.LogWarning("Geen ruimte meer om bomen te plaatsen. Spawning gestopt.");
                return;
            }
        }
    }

    void SpawnRanden()
    {
        float halfX = areaSize.x / 2f;
        float halfZ = areaSize.z / 2f;

        for (int i = 0; i < bomenPerRand; i++)
        {
            float t = (float)i / (bomenPerRand - 1);

            TrySpawnEdge(new Vector3(Mathf.Lerp(-halfX, halfX, t), 0, -halfZ));
            TrySpawnEdge(new Vector3(Mathf.Lerp(-halfX, halfX, t), 0, halfZ));
            TrySpawnEdge(new Vector3(-halfX, 0, Mathf.Lerp(-halfZ, halfZ, t)));
            TrySpawnEdge(new Vector3(halfX, 0, Mathf.Lerp(-halfZ, halfZ, t)));
        }
    }

    void TrySpawnEdge(Vector3 localPos)
    {
        Vector3 spawnPos = transform.position + localPos;

        if (IsValidPosition(spawnPos))
        {
            SpawnBoom(spawnPos);
        }
    }

    void SpawnBoom(Vector3 pos)
    {
        GameObject prefab = boomPrefabs[Random.Range(0, boomPrefabs.Length)];

        Quaternion rot = randomRotation
            ? Quaternion.Euler(0f, Random.Range(0f, 360f), 0f)
            : Quaternion.identity;

        Instantiate(prefab, pos, rot, transform);
        allSpawnedPositions.Add(pos);
    }

    bool IsValidPosition(Vector3 pos)
    {
        foreach (Vector3 p in allSpawnedPositions)
        {
            if (Vector3.Distance(pos, p) < minAfstand)
                return false;
        }
        return true;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, areaSize);
    }
}