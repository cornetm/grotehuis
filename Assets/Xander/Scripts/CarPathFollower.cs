using UnityEngine;

public class CarPathFollower : MonoBehaviour
{
    [SerializeField] private RoadTileManager tileManager;

    [Header("Movement")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float reachDistance = 1.5f;

    [Header("Steering")]
    [SerializeField] private float rotationLerp = 8f;
    [SerializeField] private int lookAhead = 2; // 1-3 is meestal nice

    private int localIndex = 0;

    void Update()
    {
        if (tileManager == null) return;
        var path = tileManager.PathQueue;
        if (path == null || path.Count == 0) return;

        // Clamp index
        if (localIndex >= path.Count) localIndex = path.Count - 1;

        Transform target = path[localIndex];
        Vector3 to = target.position - transform.position;
        float dist = to.magnitude;

        // Move forward richting target
        Vector3 dir = (dist > 0.001f) ? to / dist : transform.forward;
        transform.position += dir * speed * Time.deltaTime;

        // Lookahead voor smoother steering
        int aheadIndex = Mathf.Min(localIndex + lookAhead, path.Count - 1);
        Vector3 lookPoint = path[aheadIndex].position;
        Vector3 lookDir = (lookPoint - transform.position);
        if (lookDir.sqrMagnitude > 0.0001f)
        {
            Quaternion desired = Quaternion.LookRotation(lookDir.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, desired, rotationLerp * Time.deltaTime);
        }

        // Next waypoint?
        if (dist <= reachDistance)
        {
            localIndex++;

            // Als we ver in de queue zijn: consume om lijst klein te houden
            if (localIndex > 10)
            {
                tileManager.ConsumeWaypointsUpTo(localIndex - 5);
                localIndex = 5;
            }
        }
    }
}
