using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class MeshFoliageScatter : MonoBehaviour
{
    [Header("Target")]
    public Collider targetCollider;
    public LayerMask groundLayer = ~0;

    [Header("Prefabs")]
    public GameObject[] prefabs;

    [Header("Generation")]
    [Min(1)] public int amountToSpawn = 200;
    [Min(1)] public int maxAttempts = 5000;
    public bool alignToSurfaceNormal = true;
    public bool randomYRotation = true;
    public Vector2 randomScaleRange = new Vector2(0.8f, 1.2f);

    [Header("Collision Avoidance")]
    [Min(0.01f)] public float clearanceRadius = 0.5f;
    public LayerMask blockingLayers = ~0;
    [Min(0.1f)] public float rayStartHeight = 10f;
    [Range(0f, 90f)] public float maxSlope = 45f;

    [Header("Parenting")]
    public Transform generatedParent;
    public string generatedParentName = "Generated Foliage";

    private readonly List<Vector3> spawnedPositions = new List<Vector3>();

    public void GenerateFoliage()
    {
        if (targetCollider == null)
        {
            Debug.LogWarning("No targetCollider assigned.", this);
            return;
        }

        if (prefabs == null || prefabs.Length == 0)
        {
            Debug.LogWarning("No prefabs assigned.", this);
            return;
        }

        EnsureParentExists();
        spawnedPositions.Clear();

        Bounds bounds = targetCollider.bounds;
        int spawned = 0;
        int attempts = 0;

        while (spawned < amountToSpawn && attempts < maxAttempts)
        {
            attempts++;

            Vector3 rayStart = GetRandomPointAboveBounds(bounds);

            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, bounds.size.y + rayStartHeight * 2f, groundLayer, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider != targetCollider)
                    continue;

                float slope = Vector3.Angle(hit.normal, Vector3.up);
                if (slope > maxSlope)
                    continue;

                if (!IsPositionClear(hit.point))
                    continue;

                SpawnPrefabAtHit(hit);
                spawned++;
                spawnedPositions.Add(hit.point);
            }
        }

        Debug.Log($"Foliage generation done. Spawned {spawned}/{amountToSpawn} after {attempts} attempts.", this);
    }

    public void ClearGeneratedFoliage()
    {
        Transform parentToClear = generatedParent;

        if (parentToClear == null)
        {
            Transform found = transform.Find(generatedParentName);
            if (found != null)
                parentToClear = found;
        }

        if (parentToClear == null)
        {
            Debug.Log("No generated foliage parent found to clear.", this);
            return;
        }

        List<GameObject> children = new List<GameObject>();
        foreach (Transform child in parentToClear)
            children.Add(child.gameObject);

        for (int i = children.Count - 1; i >= 0; i--)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                UnityEditor.Undo.DestroyObjectImmediate(children[i]);
            else
                Destroy(children[i]);
#else
            DestroyImmediate(children[i]);
#endif
        }

        Debug.Log("Cleared generated foliage.", this);
    }

    private Vector3 GetRandomPointAboveBounds(Bounds bounds)
    {
        float x = Random.Range(bounds.min.x, bounds.max.x);
        float z = Random.Range(bounds.min.z, bounds.max.z);
        float y = bounds.max.y + rayStartHeight;
        return new Vector3(x, y, z);
    }

    private bool IsPositionClear(Vector3 point)
    {
        float sqrRadius = clearanceRadius * clearanceRadius;

        for (int i = 0; i < spawnedPositions.Count; i++)
        {
            if ((spawnedPositions[i] - point).sqrMagnitude < sqrRadius)
                return false;
        }

        Collider[] hits = Physics.OverlapSphere(point, clearanceRadius, blockingLayers, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i] == targetCollider)
                continue;

            return false;
        }

        return true;
    }

    private void SpawnPrefabAtHit(RaycastHit hit)
    {
        GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
        if (prefab == null) return;

        Quaternion rotation = alignToSurfaceNormal
            ? Quaternion.FromToRotation(Vector3.up, hit.normal)
            : Quaternion.identity;

        if (randomYRotation)
        {
            Quaternion yRot = Quaternion.AngleAxis(Random.Range(0f, 360f), alignToSurfaceNormal ? hit.normal : Vector3.up);
            rotation = yRot * rotation;
        }

        float scale = Random.Range(randomScaleRange.x, randomScaleRange.y);

        GameObject instance;

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            instance = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab, generatedParent);
            if (instance == null)
                instance = Instantiate(prefab, generatedParent);

            UnityEditor.Undo.RegisterCreatedObjectUndo(instance, "Generate Foliage");
        }
        else
        {
            instance = Instantiate(prefab, generatedParent);
        }
#else
        instance = Instantiate(prefab, generatedParent);
#endif

        instance.transform.position = hit.point;
        instance.transform.rotation = rotation;
        instance.transform.localScale *= scale;
    }

    private void EnsureParentExists()
    {
        if (generatedParent != null)
            return;

        Transform found = transform.Find(generatedParentName);
        if (found != null)
        {
            generatedParent = found;
            return;
        }

        GameObject parentObj = new GameObject(generatedParentName);
        parentObj.transform.SetParent(transform);

#if UNITY_EDITOR
        if (!Application.isPlaying)
            UnityEditor.Undo.RegisterCreatedObjectUndo(parentObj, "Create Foliage Parent");
#endif

        generatedParent = parentObj.transform;
    }
}