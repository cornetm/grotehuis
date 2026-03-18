using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class MonsterSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject[] MonsterPrefabs;  // Alle normale monsters
    [Range(0, 100)]
    public float SpawnChance = 30f;

    [Header("Optional Spawn Points")]
    public Transform[] SpawnPoints;

    [Header("Special Skull Monster")]
    public GameObject SkullMonsterPrefab;  // Speciaal veld voor doodshoofd
    public Transform SkullSpawnPoint;      // Speciaal spawnpoint voor doodshoofd

    [Header("Respawn Boundary (drag your BoxCollider here)")]
    public Collider RoomBoundary;
    public float boundaryActiveTime = 5f;

    [HideInInspector] public bool RoomHasMonster = false;

    private GameObject spawnedMonster;
    private bool hasSpawned = false;
    private Transform monsterParent;

    void Awake()
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        if (RoomBoundary != null)
            RoomBoundary.isTrigger = true;
    }

    public void SetParent(Transform parent)
    {
        monsterParent = parent;
    }

    public void TrySpawn(Transform parent, bool force = false, bool isForwardRoom = false, bool forceSkull = false)
    {
        if (hasSpawned || RoomHasMonster) return;

        GameObject prefabToSpawn = null;
        Transform spawnPoint = transform;

        if (forceSkull && SkullMonsterPrefab != null && SkullSpawnPoint != null)
        {
            prefabToSpawn = SkullMonsterPrefab;
            spawnPoint = SkullSpawnPoint;
        }
        else if (isForwardRoom && SkullMonsterPrefab != null && SkullSpawnPoint != null && MonsterPrefabs.Length > 0)
        {
            if (Random.value < 0.5f)
            {
                prefabToSpawn = SkullMonsterPrefab;
                spawnPoint = SkullSpawnPoint;
            }
            else
            {
                prefabToSpawn = MonsterPrefabs[Random.Range(0, MonsterPrefabs.Length)];
                spawnPoint = (SpawnPoints.Length > 0) ? SpawnPoints[Random.Range(0, SpawnPoints.Length)] : transform;
            }
        }
        else if (SkullMonsterPrefab != null && SkullSpawnPoint != null && MonsterPrefabs.Length == 0)
        {
            prefabToSpawn = SkullMonsterPrefab;
            spawnPoint = SkullSpawnPoint;
        }
        else if (MonsterPrefabs.Length > 0)
        {
            if (!force && Random.Range(0f, 100f) > SpawnChance) return;
            prefabToSpawn = MonsterPrefabs[Random.Range(0, MonsterPrefabs.Length)];
            spawnPoint = (SpawnPoints.Length > 0) ? SpawnPoints[Random.Range(0, SpawnPoints.Length)] : transform;
        }
        else
        {
            Debug.LogWarning($"MonsterSpawner in {gameObject.name} heeft geen monsters ingesteld!");
            return;
        }

        spawnedMonster = Instantiate(prefabToSpawn, spawnPoint.position, spawnPoint.rotation, parent);
        hasSpawned = true;
        RoomHasMonster = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (RoomBoundary != null && other.gameObject == spawnedMonster)
        {
            RespawnMonster();
        }
    }

    public void RespawnMonster()
    {
        if (spawnedMonster == null) return;

        Transform chosenPoint = (SpawnPoints.Length > 0) ? SpawnPoints[Random.Range(0, SpawnPoints.Length)] : transform;

        spawnedMonster.transform.position = chosenPoint.position;
        spawnedMonster.transform.rotation = chosenPoint.rotation;

        Rigidbody rb = spawnedMonster.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (RoomBoundary != null)
            StartCoroutine(DisableBoundaryAfterSeconds());
    }

    private IEnumerator DisableBoundaryAfterSeconds()
    {
        Collider col = RoomBoundary;
        yield return new WaitForSeconds(boundaryActiveTime);
        if (col != null)
            col.enabled = false;
    }
}