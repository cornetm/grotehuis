using UnityEngine;

[ExecuteAlways]
public class MonsterSkullAI : MonoBehaviour
{
    [Header("Detection Ranges")]
    public float alertRadius = 5f;
    public float chaseRadius = 10f;
    public float viewHeightOffset = 1.5f;
    public int verticalSteps = 3; // aantal raycasts van beneden naar boven

    [Header("Animator")]
    public Animator animator; // sleep hier je Animator component in
    public string alertTrigger = "AlertTrigger";  // trigger voor alert animatie
    public string attackTrigger = "AttackTrigger"; // trigger voor attack animatie

    private Transform player;
    private float playerHeight = 2f;
    private bool hasPlayedAlert = false;
    private bool isChasing = false;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;

            // Automatisch spelerhoogte detecteren
            Collider col = player.GetComponent<Collider>();
            if (col != null)
                playerHeight = col.bounds.size.y;
        }
    }

    void Update()
    {
        if (player == null || animator == null) return;

        Vector3 eyePos = transform.position + Vector3.up * viewHeightOffset;
        Vector3 dirToPlayer = player.position - eyePos;
        dirToPlayer.y = 0;

        float horizontalDistance = dirToPlayer.magnitude;

        bool inAlert = horizontalDistance <= alertRadius && HasLineOfSight(eyePos, player.position, playerHeight);
        bool inChase = horizontalDistance <= chaseRadius && HasLineOfSight(eyePos, player.position, playerHeight);

        // --------------------------
        // Alert animatie 1x afspelen
        // --------------------------
        if (inAlert && !hasPlayedAlert)
        {
            animator.SetTrigger(alertTrigger);
            hasPlayedAlert = true;
            Debug.Log("[MonsterSkullAI] Player entered ALERT radius!");
        }

        if (!inAlert)
        {
            hasPlayedAlert = false;
        }

        // --------------------------
        // Attack animatie zolang speler in chase radius
        // --------------------------
        if (inChase && !isChasing)
        {
            animator.SetTrigger(attackTrigger);
            isChasing = true;
            Debug.Log("[MonsterSkullAI] Player entered CHASE radius!");
        }

        if (!inChase)
        {
            isChasing = false;
        }

        // Buiten beide radius → geen animatie wordt afgespeeld
    }

    /// <summary>
    /// Controleer line-of-sight met meerdere verticale raycasts
    /// </summary>
    bool HasLineOfSight(Vector3 from, Vector3 playerPos, float height)
    {
        for (int i = 0; i < verticalSteps; i++)
        {
            float y = (height / (verticalSteps - 1)) * i;
            Vector3 target = playerPos + Vector3.up * y;

            if (Physics.Linecast(from, target, out RaycastHit hit))
            {
                // Alleen muren blokkeren (Room tag, isTrigger = false)
                if (!hit.collider.isTrigger && hit.collider.CompareTag("Room"))
                    continue; // ray geblokkeerd door muur
            }

            return true; // één ray kan speler bereiken
        }

        return false; // alle rays geblokkeerd
    }

    void OnDrawGizmosSelected()
    {
        DrawVisibleRange(alertRadius, Color.yellow);
        DrawVisibleRange(chaseRadius, Color.red);
    }

    void DrawVisibleRange(float radius, Color color)
    {
        Gizmos.color = color;

        int steps = 60;
        float angleStep = 360f / steps;
        Vector3 eyePos = transform.position + Vector3.up * viewHeightOffset;

        for (int i = 0; i < steps; i++)
        {
            float angle = i * angleStep;
            Vector3 dir = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                0,
                Mathf.Sin(angle * Mathf.Deg2Rad)
            );

            Vector3 endPoint = eyePos + dir * radius;

            if (Physics.Raycast(eyePos, dir, out RaycastHit hit, radius))
            {
                if (!hit.collider.isTrigger && hit.collider.CompareTag("Room"))
                    endPoint = hit.point;
            }

            Gizmos.DrawLine(eyePos, endPoint);
        }
    }
}