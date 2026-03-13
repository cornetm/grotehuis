using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MonsterSimpleAI : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 2.5f;
    public float sprintSpeed = 5.5f;

    [Header("Wander")]
    public float wanderRadius = 15f;
    public float wanderInterval = 4f;

    [Header("Detection")]
    public float chaseRadius = 10f;
    public int rayCount = 120;
    public float viewHeightOffset = 1.5f;

    private Transform player;
    private Vector3 targetPosition;
    private float timer;
    private bool chasing;

    private Vector3[] rayPoints;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        PickNewDestination();
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

            if (timer >= wanderInterval || Vector3.Distance(transform.position, targetPosition) < 1f)
                PickNewDestination();

            MoveTowards(targetPosition, walkSpeed);
        }
    }

    void MoveTowards(Vector3 target, float speed)
    {
        Vector3 direction = (target - transform.position).normalized;

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(direction),
                5f * Time.deltaTime
            );
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            speed * Time.deltaTime
        );
    }

    void PickNewDestination()
    {
        timer = 0f;

        Vector2 random = Random.insideUnitCircle * wanderRadius;

        targetPosition = new Vector3(
            transform.position.x + random.x,
            transform.position.y,
            transform.position.z + random.y
        );
    }

    bool IsPlayerInRadius()
    {
        Vector3 monsterEye = transform.position + Vector3.up * viewHeightOffset;
        Vector3 playerPos = player.position + Vector3.up * 0.5f;

        float dist = Vector3.Distance(monsterEye, playerPos);

        if (dist > chaseRadius)
            return false;

        RaycastHit[] hits = Physics.RaycastAll(monsterEye, (playerPos - monsterEye).normalized, chaseRadius);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.isTrigger)
                continue;

            if (hit.collider.CompareTag("Room"))
                return false;

            if (hit.transform == player)
                return true;
        }

        return false;
    }

    void UpdateRayPoints()
    {
        Vector3 center = transform.position + Vector3.up * viewHeightOffset;
        rayPoints = new Vector3[rayCount];

        for (int i = 0; i < rayCount; i++)
        {
            float angle = i * 360f / rayCount;
            Vector3 dir = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad));

            RaycastHit[] hits = Physics.RaycastAll(center, dir, chaseRadius);
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            bool hitRoom = false;

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.isTrigger)
                    continue;

                if (hit.collider.CompareTag("Room"))
                {
                    rayPoints[i] = hit.point;
                    hitRoom = true;
                    break;
                }
            }

            if (!hitRoom)
                rayPoints[i] = center + dir * chaseRadius;
        }
    }

    void OnDrawGizmosSelected()
    {
        UpdateRayPoints();
        if (rayPoints == null || rayPoints.Length == 0) return;

        Gizmos.color = Color.red;

        for (int i = 0; i < rayPoints.Length; i++)
        {
            Vector3 from = rayPoints[i];
            Vector3 to = rayPoints[(i + 1) % rayPoints.Length];
            Gizmos.DrawLine(from, to);
        }

#if UNITY_EDITOR
        Handles.color = new Color(1f, 0f, 0f, 0.1f);
        Handles.DrawAAConvexPolygon(rayPoints);
#endif
    }
}