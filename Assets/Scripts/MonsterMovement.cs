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
    public float idleChance = 0.3f; // kans om even stil te staan
    public float idleDurationMin = 1f;
    public float idleDurationMax = 3f;

    [Header("Detection")]
    public float chaseRadius = 10f;
    public float viewHeightOffset = 1.5f;

    [Header("Wall Avoidance")]
    public float wallCheckDistance = 1f;

    [Header("Animator")]
    public Animator animator; // sleep je Animator component hier

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
        if (playerObj != null) player = playerObj.transform;

        PickDestinationInCurrentOrAnotherRoom();
    }

    void Update()
    {
        if (player == null) return;

        float currentSpeed = 0f;

        // Check of monster de speler ziet
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

                // Random kans om even idle te gaan
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

        // Wall check gebaseerd op BoxCollider
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
        Vector3 dir = player.position - eyePos;
        dir.y = 0;

        if (dir.magnitude > chaseRadius) return false;

        if (Physics.Linecast(eyePos, player.position, out RaycastHit hit))
        {
            if (!hit.collider.isTrigger && hit.collider.CompareTag("Room"))
                return false;
        }

        return true;
    }
}