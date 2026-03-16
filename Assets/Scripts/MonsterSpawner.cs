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
    public Collider RoomBoundary;
    public float boundaryActiveTime = 5f;

    private GameObject spawnedMonster;
    private Transform monsterParent;

    void Awake()
    {
        // Zorg dat boundary collider trigger is
        if (RoomBoundary != null)
            RoomBoundary.isTrigger = true;
    }

    public void SetParent(Transform parent)
    {
        monsterParent = parent;
    }

    // Handmatige spawn methode
    public void TrySpawn(bool force = false)
    {
        if (spawnedMonster != null) return;
        if (!force && Random.Range(0f, 100f) > SpawnChance) return;
        if (MonsterPrefabs.Length == 0) return;

        GameObject prefab = MonsterPrefabs[Random.Range(0, MonsterPrefabs.Length)];
        Transform chosenPoint = (SpawnPoints.Length > 0) ? SpawnPoints[Random.Range(0, SpawnPoints.Length)] : transform;

        spawnedMonster = Instantiate(prefab, chosenPoint.position, chosenPoint.rotation, monsterParent);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Alleen check voor respawn boundary
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