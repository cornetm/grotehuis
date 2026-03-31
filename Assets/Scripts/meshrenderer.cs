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
        if (!StartTrigger.GameStarted) return;

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

            if (distance <= viewRadius)
                mr.enabled = true;
            else
                mr.enabled = false;
        }
    }

    // 👇 VISUELE RADIUS (EDITOR)
    void OnDrawGizmos()
    {
        if (player == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(player.position, viewRadius);
    }
}