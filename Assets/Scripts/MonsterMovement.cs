using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(CharacterController))]
public class MonsterSimpleAI : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 2.5f;
    public float sprintSpeed = 5.5f;

    [Header("Wander")]
    public float wanderInterval = 4f;

    [Header("Detection")]
    public float chaseRadius = 10f;
    public float viewHeightOffset = 1.5f;

    [Header("Wall Avoidance")]
    public float wallCheckDistance = 1f;

    private Transform player;
    private Vector3 targetPosition;
    private float timer;
    private bool chasing;

    private CharacterController controller;
    private Vector3 velocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        PickDestinationInCurrentOrAnotherRoom();
    }

    void Update()
    {
        if (player == null) return;

        if (IsPlayerInRadius())
        {
            chasing = true;
            targetPosition = player.position;
            MoveTowards(targetPosition, sprintSpeed);
        }
        else
        {
            chasing = false;
            timer += Time.deltaTime;

            if (Vector3.Distance(transform.position, targetPosition) < 0.5f || timer >= wanderInterval)
                PickDestinationInCurrentOrAnotherRoom();

            MoveTowards(targetPosition, walkSpeed);
        }
    }

    void MoveTowards(Vector3 target, float speed)
    {
        Vector3 direction = target - transform.position;
        direction.y = 0;

        if (direction.sqrMagnitude < 0.01f) return;
        direction.Normalize();

        // muur detectie
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, direction, out RaycastHit hit, wallCheckDistance))
        {
            if (!hit.collider.isTrigger && hit.collider.CompareTag("Room"))
            {
                PickDestinationInCurrentOrAnotherRoom();
                return;
            }
        }

        // CharacterController movement
        velocity = direction * speed;
        velocity.y = -9.81f * Time.deltaTime; // gravity
        controller.Move(velocity * Time.deltaTime);

        // Rotatie
        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 5f * Time.deltaTime);
        }
    }

    void PickDestinationInCurrentOrAnotherRoom()
    {
        timer = 0f;

        GameObject[] rooms = GameObject.FindGameObjectsWithTag("Room");
        if (rooms.Length == 0) return;

        GameObject chosenRoom = rooms[Random.Range(0, rooms.Length)];

        Collider roomCol = chosenRoom.GetComponent<Collider>();
        if (roomCol == null) return;

        Vector3 min = roomCol.bounds.min;
        Vector3 max = roomCol.bounds.max;

        targetPosition = new Vector3(
            Random.Range(min.x, max.x),
            transform.position.y,
            Random.Range(min.z, max.z)
        );
    }

    bool IsPlayerInRadius()
    {
        if (player == null) return false;

        Vector3 eyePos = transform.position + Vector3.up * viewHeightOffset;
        Vector3 direction = player.position - eyePos;
        direction.y = 0;

        if (direction.magnitude > chaseRadius) return false;

        if (Physics.Linecast(eyePos, player.position, out RaycastHit hit))
        {
            if (!hit.collider.isTrigger && hit.collider.CompareTag("Room"))
                return false;
        }

        return true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(targetPosition, 0.3f);
    }
}