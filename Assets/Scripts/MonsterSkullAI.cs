using UnityEngine;

[ExecuteAlways]
public class MonsterSkullAI : MonoBehaviour
{
    [Header("Detection Ranges")]
    public float alertRadius = 5f;
    public float chaseRadius = 10f;
    public float viewHeightOffset = 1.5f;
    public int verticalSteps = 3;

    [Header("Animator")]
    public Animator animator;
    public string alertTrigger = "AlertTrigger";
    public string attackTrigger = "AttackTrigger";

    [Header("Chase Settings")]
    public float moveSpeed = 3.5f;  // snelheid tijdens chase

    private Transform player;
    private float playerHeight = 2f;

    private bool alertTriggered = false;
    private bool attackTriggered = false;
    private bool alertEnabled = true;
    private bool chaseEnabled = true;

    private CharacterController controller;
    private Vector3 verticalVelocity;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        controller = GetComponent<CharacterController>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            Collider col = player.GetComponent<Collider>();
            if (col != null)
                playerHeight = col.bounds.size.y;
        }
    }

    void Update()
    {
        // Detectie en triggers alleen in Play Mode
        if (!Application.isPlaying) return;
        if (player == null || animator == null) return;

        Vector3 eyePos = transform.position + Vector3.up * viewHeightOffset;
        Vector3 horizontalDir = player.position - eyePos;
        horizontalDir.y = 0;
        float distance = horizontalDir.magnitude;

        bool canSeePlayer = HasLineOfSight(eyePos, player.position, playerHeight);

        // --------------------------
        // CHASE / ATTACK heeft hoogste prioriteit
        // --------------------------
        if (distance <= chaseRadius && canSeePlayer && !attackTriggered)
        {
            animator.speed = 1f;
            animator.SetTrigger(attackTrigger);
            attackTriggered = true;

            alertEnabled = false;
            chaseEnabled = false;

            Debug.Log("ATTACK TRIGGERED IMMEDIATELY");
        }

        // --------------------------
        // ALERT animatie (1x)
        // --------------------------
        if (alertEnabled && distance <= alertRadius && canSeePlayer && !alertTriggered)
        {
            animator.speed = 1f;
            animator.SetTrigger(alertTrigger);
            alertTriggered = true;

            alertEnabled = false;
            chaseEnabled = true;

            Debug.Log("ALERT TRIGGERED ONE TIME");
        }

        // Freeze laatste frame van alert animatie
        if (alertTriggered && !attackTriggered)
        {
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            if (state.IsName("Alert") && state.normalizedTime >= 1f)
                animator.speed = 0f;
        }

        // --------------------------
        // Chase movement zodra attack animatie actief is
        // --------------------------
        if (attackTriggered)
        {
            MoveTowardsPlayer();
        }
    }

    void MoveTowardsPlayer()
    {
        if (player == null || controller == null) return;

        Vector3 dir = player.position - transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude < 0.01f) return;
        dir.Normalize();

        Vector3 horizontalVelocity = dir * moveSpeed;

        if (controller.isGrounded)
            verticalVelocity.y = -1f;
        else
            verticalVelocity.y += Physics.gravity.y * Time.deltaTime;

        Vector3 totalVelocity = horizontalVelocity + verticalVelocity;
        controller.Move(totalVelocity * Time.deltaTime);

        if (dir.sqrMagnitude > 0.01f)
        {
            Quaternion lookRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, 5f * Time.deltaTime);
        }
    }

    bool HasLineOfSight(Vector3 from, Vector3 playerPos, float height)
    {
        for (int i = 0; i < verticalSteps; i++)
        {
            float y = (height / (verticalSteps - 1)) * i;
            Vector3 target = playerPos + Vector3.up * y;

            if (Physics.Linecast(from, target, out RaycastHit hit))
            {
                if (!hit.collider.isTrigger && hit.collider.CompareTag("Room"))
                    continue; // muur blokkeert deze ray
            }

            return true; // minstens 1 ray bereikt speler
        }

        return false;
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
            Vector3 dir = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad));
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