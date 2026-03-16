using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class MonsterSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject[] MonsterPrefabs;
    [Range(0, 100)]
    public float SpawnChance = 30f;

    [Header("Optional Spawn Points")]
    public Transform[] SpawnPoints;

    [Header("Respawn Boundary (drag your BoxCollider here)")]
    public Collider RoomBoundary;  // sleep hier de box collider van de kamer in
    public float boundaryActiveTime = 5f;

    private GameObject spawnedMonster;
    private bool hasSpawned = false;
    private bool playerInRange = false;
    private Transform monsterParent;

    void Awake()
    {
        // Trigger collider voor spawn
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        // Zorg dat boundary collider trigger is
        if (RoomBoundary != null)
            RoomBoundary.isTrigger = true;
    }

    void Update()
    {
        // Spawn pas als speler in de trigger is
        if (!playerInRange || hasSpawned || monsterParent == null) return;
        TrySpawn(monsterParent);
    }

    public void SetParent(Transform parent)
    {
        monsterParent = parent;
    }

    public void TrySpawn(Transform parent, bool force = false)
    {
        if (hasSpawned) return;
        if (!force && Random.Range(0f, 100f) > SpawnChance) return;

        if (MonsterPrefabs.Length == 0) return;

        GameObject prefab = MonsterPrefabs[Random.Range(0, MonsterPrefabs.Length)];
        Transform chosenPoint = (SpawnPoints.Length > 0) ? SpawnPoints[Random.Range(0, SpawnPoints.Length)] : transform;

        spawnedMonster = Instantiate(prefab, chosenPoint.position, chosenPoint.rotation, parent);

        hasSpawned = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }

        // Monster raakt zijn respawn boundary
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

        // Reset velocity als Rigidbody
        Rigidbody rb = spawnedMonster.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Zet boundary uit na delay
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