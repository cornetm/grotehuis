using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Rigidbody))]
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
    public float verticalRange = 3f;      // afstand tussen onderste en bovenste layer
    public int horizontalRays = 36;       // aantal rays rondom 360 graden
    public int verticalRays = 3;          // aantal lagen van onder naar boven
    public float viewHeightOffset = 1.5f;
    public float verticalAngle = 15f;     // hoek omhoog/omlaag voor onderste/bovenste layer

    private Transform player;
    private Vector3 targetPosition;
    private float timer;
    private bool chasing;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.useGravity = true;

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
        Vector3 direction = target - transform.position;
        direction.y = 0;
        if (direction.sqrMagnitude < 0.01f) return;

        Vector3 move = direction.normalized * speed * Time.deltaTime;
        rb.MovePosition(transform.position + move);

        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 5f * Time.deltaTime);
    }

    void PickNewDestination()
    {
        timer = 0f;
        Vector2 random = Random.insideUnitCircle * wanderRadius;
        targetPosition = new Vector3(transform.position.x + random.x, transform.position.y, transform.position.z + random.y);
    }

    bool IsPlayerInRadius()
    {
        if (player == null) return false;

        Vector3 eyePos = transform.position + Vector3.up * viewHeightOffset;

        for (int v = 0; v < verticalRays; v++)
        {
            float yOffset = -verticalRange / 2f + v * (verticalRange / (verticalRays - 1));
            Vector3 layerOrigin = eyePos + Vector3.up * yOffset;

            float layerAngle = 0f;
            if (v == 0) layerAngle = verticalAngle;      // onderste layer → schuin omhoog
            if (v == 2) layerAngle = -verticalAngle;     // bovenste layer → schuin omlaag
            // middelste layer 1 → horizontaal (angle = 0)

            for (int h = 0; h < horizontalRays; h++)
            {
                float angle = h * 360f / horizontalRays;
                Vector3 dir = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;
                dir = Quaternion.AngleAxis(layerAngle, Vector3.Cross(Vector3.up, dir)) * dir; // kantel omhoog/omlaag

                RaycastHit hit;
                if (Physics.Raycast(layerOrigin, dir, out hit, chaseRadius))
                {
                    if (hit.collider.isTrigger)
                        continue;

                    if (hit.transform == player)
                        return true;
                }
            }
        }

        return false;
    }

    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        Vector3 eyePos = transform.position + Vector3.up * viewHeightOffset;
        Gizmos.color = Color.red;

        for (int v = 0; v < verticalRays; v++)
        {
            float yOffset = -verticalRange / 2f + v * (verticalRange / (verticalRays - 1));
            Vector3 layerOrigin = eyePos + Vector3.up * yOffset;

            float layerAngle = 0f;
            if (v == 0) layerAngle = verticalAngle;
            if (v == 2) layerAngle = -verticalAngle;

            for (int h = 0; h < horizontalRays; h++)
            {
                float angle = h * 360f / horizontalRays;
                Vector3 dir = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;
                dir = Quaternion.AngleAxis(layerAngle, Vector3.Cross(Vector3.up, dir)) * dir;

                RaycastHit hit;
                Vector3 end = layerOrigin + dir * chaseRadius;
                if (Physics.Raycast(layerOrigin, dir, out hit, chaseRadius))
                {
                    if (!hit.collider.isTrigger)
                        end = hit.point;
                }

                Gizmos.DrawLine(layerOrigin, end);
            }
        }
    }
}