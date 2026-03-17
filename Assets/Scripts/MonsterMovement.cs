using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Animator))]
public class MonsterSimpleAI : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 2.5f;
    public float sprintSpeed = 5.5f;

    [Header("Wander")]
    public float wanderInterval = 4f;
    public float idleChance = 0.3f;
    public float idleDurationMin = 1f;
    public float idleDurationMax = 3f;

    [Header("Detection")]
    public float chaseRadius = 10f;
    public float viewHeightOffset = 1.5f;

    [Header("Wall Avoidance")]
    public float wallCheckDistance = 1f;

    [Header("Animator")]
    public Animator animator;

    private Transform player;
    private Vector3 targetPosition;
    private float timer;
    private bool chasing;

    private CharacterController controller;
    private BoxCollider boxCollider;
    private Vector3 verticalVelocity;

    private bool isIdling = false;
    private float idleTimer = 0f;
    private float idleDuration = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        boxCollider = GetComponent<BoxCollider>();
        if (animator == null) animator = GetComponent<Animator>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        PickDestinationInCurrentOrAnotherRoom();
    }

    void Update()
    {
        if (player == null) return;

        float currentSpeed = 0f;

        if (IsPlayerInRadius())
        {
            chasing = true;
            isIdling = false;
            targetPosition = player.position;
            currentSpeed = sprintSpeed;
        }
        else
        {
            chasing = false;

            if (isIdling)
            {
                idleTimer += Time.deltaTime;
                currentSpeed = 0f;

                if (idleTimer >= idleDuration)
                {
                    isIdling = false;
                    PickDestinationInCurrentOrAnotherRoom();
                }
            }
            else
            {
                timer += Time.deltaTime;
                currentSpeed = walkSpeed;

                if (Random.value < idleChance * Time.deltaTime)
                {
                    isIdling = true;
                    idleDuration = Random.Range(idleDurationMin, idleDurationMax);
                    idleTimer = 0f;
                    currentSpeed = 0f;
                }

                if (Vector3.Distance(transform.position, targetPosition) < 0.5f || timer >= wanderInterval)
                    PickDestinationInCurrentOrAnotherRoom();
            }
        }

        MoveTowards(targetPosition, currentSpeed);
        UpdateAnimation(currentSpeed);
    }

    void MoveTowards(Vector3 target, float speed)
    {
        Vector3 dir = target - transform.position;
        dir.y = 0;

        if (dir.sqrMagnitude < 0.01f || speed <= 0f) return;
        dir.Normalize();

        Vector3 rayOrigin = transform.position + Vector3.up * boxCollider.bounds.extents.y;
        float rayDistance = wallCheckDistance + boxCollider.bounds.extents.x;

        if (Physics.Raycast(rayOrigin, dir, out RaycastHit hit, rayDistance))
        {
            if (!hit.collider.isTrigger && hit.collider.CompareTag("Room"))
            {
                PickDestinationInCurrentOrAnotherRoom();
                return;
            }
        }

        Vector3 horizontalVelocity = dir * speed;

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

    void UpdateAnimation(float speed)
    {
        if (animator == null) return;

        animator.ResetTrigger("Idle");
        animator.ResetTrigger("Walk");
        animator.ResetTrigger("Run");

        if (speed < 0.1f)
            animator.SetTrigger("Idle");
        else if (!chasing)
            animator.SetTrigger("Walk");
        else
            animator.SetTrigger("Run");
    }

    void PickDestinationInCurrentOrAnotherRoom()
    {
        timer = 0f;

        GameObject[] rooms = GameObject.FindGameObjectsWithTag("Room");
        if (rooms.Length == 0) return;

        GameObject chosenRoom = rooms[Random.Range(0, rooms.Length)];
        Collider col = chosenRoom.GetComponent<Collider>();
        if (col == null) return;

        targetPosition = new Vector3(
            Random.Range(col.bounds.min.x, col.bounds.max.x),
            transform.position.y,
            Random.Range(col.bounds.min.z, col.bounds.max.z)
        );
    }

    bool IsPlayerInRadius()
    {
        if (player == null) return false;

        Vector3 eyePos = transform.position + Vector3.up * viewHeightOffset;

        // Automatische spelerhoogte
        float playerHeight = 2f;
        Collider col = player.GetComponent<Collider>();
        if (col != null)
            playerHeight = col.bounds.size.y;

        // Horizontale afstand voor radius check
        Vector3 horizontalDir = player.position - eyePos;
        horizontalDir.y = 0;
        if (horizontalDir.magnitude > chaseRadius) return false;

        // Line-of-sight met meerdere verticale raycasts
        int verticalSteps = 3;
        for (int i = 0; i < verticalSteps; i++)
        {
            float y = (playerHeight / (verticalSteps - 1)) * i;
            Vector3 target = player.position + Vector3.up * y;

            if (Physics.Linecast(eyePos, target, out RaycastHit hit))
            {
                if (!hit.collider.isTrigger && hit.collider.CompareTag("Room"))
                    continue; // muur blokkeert
            }

            return true; // één ray kan speler raken
        }

        return false;
    }

    // -------------------------
    // Gizmos voor inspectie
    // -------------------------
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        DrawDetectionRange(chaseRadius, Color.red);
    }

    void DrawDetectionRange(float radius, Color color)
    {
        Gizmos.color = color;
        Vector3 eyePos = transform.position + Vector3.up * viewHeightOffset;

        int steps = 60;
        float angleStep = 360f / steps;

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