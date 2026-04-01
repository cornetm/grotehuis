using UnityEngine;

public class meshrenderer : MonoBehaviour
{
    [Header("Player Settings")]
    public Transform player;
    public float viewRadius = 15f;

    [Header("Capsule Settings")]
    public float newRadius = 0.8f;

    private CapsuleCollider playerCapsule;

    void Start()
    {
        if (player != null)
        {
            playerCapsule = player.GetComponent<CapsuleCollider>();
        }
    }

    void Update()
    {
        // ✅ Alleen actief wanneer deleteObject is geraakt
        if (!PlayerMovement.GameStarted) return;

        if (playerCapsule != null)
        {
            playerCapsule.radius = newRadius;
        }

        if (player == null) return;

        MeshRenderer[] allMeshes = FindObjectsOfType<MeshRenderer>();

        foreach (MeshRenderer mr in allMeshes)
        {
            if (mr == null) continue;

            float distance = Vector3.Distance(player.position, mr.transform.position);

            // Mesh aan/uit afhankelijk van viewRadius
            mr.enabled = distance <= viewRadius;
        }
    }

    // 👇 Visuele radius (editor)
    void OnDrawGizmos()
    {
        if (player == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(player.position, viewRadius);
    }
}