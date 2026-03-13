using System.Collections.Generic;
using UnityEngine;

public class SplineCarPathFollower : MonoBehaviour
{
    [SerializeField] private RoadTileManager tileManager;

    [Header("Motion")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float rotationLerp = 10f;

    [Header("Steering Wheel (Yaw-rate based, smoothed)")]
    [SerializeField] private Transform steeringWheel;
    [SerializeField] private float maxWheelAngle = 450f;        // 360-540 normaal
    [SerializeField] private bool invertWheel = false;

    [Tooltip("Deg/sec yaw waarbij stuur full lock is. Lager = sneller full lock.")]
    [SerializeField] private float yawRateForFullLock = 60f;    // deg/sec

    [Tooltip("Low-pass smoothing voor yaw-rate (seconden). 0.10-0.25 is vaak top.")]
    [SerializeField] private float yawRateSmoothTime = 0.15f;

    [Tooltip("Max snelheid waarmee het stuur mag draaien (deg/sec). 600-1200 voelt realistisch.")]
    [SerializeField] private float maxWheelSpeed = 900f;

    [Tooltip("Extra multiplier (fine tune).")]
    [SerializeField] private float steeringGain = 1f;

    [Header("Spline")]
    [SerializeField] private float minPointsNeeded = 4;

    private int i = 0;
    private float t = 0f;

    private float wheelBaseLocalZ;
    private float currentWheelAngle;

    private float lastYaw;
    private float smoothedYawRate;  // deg/sec (filtered)
    private float yawRateVel;       // SmoothDamp helper

    void Start()
    {
        lastYaw = NormalizeAngle(transform.eulerAngles.y);

        if (steeringWheel != null)
            wheelBaseLocalZ = steeringWheel.localEulerAngles.z;
    }

    void Update()
    {
        if (tileManager == null) return;
        IReadOnlyList<Transform> path = tileManager.PathQueue;
        if (path == null || path.Count < 4) return;

        i = Mathf.Clamp(i, 1, path.Count - 3);

        Vector3 p0 = path[i - 1].position;
        Vector3 p1 = path[i].position;
        Vector3 p2 = path[i + 1].position;
        Vector3 p3 = path[i + 2].position;

        float segLen = Vector3.Distance(p1, p2);
        if (segLen < 0.0001f) segLen = 0.0001f;

        float dtSeg = (speed * Time.deltaTime) / segLen;
        t += dtSeg;

        while (t >= 1f)
        {
            t -= 1f;
            i++;

            i = Mathf.Clamp(i, 1, path.Count - 3);

            if (i > 10)
            {
                int consume = i - 5;
                tileManager.ConsumeWaypointsUpTo(consume);
                i -= consume;
            }

            p0 = path[i - 1].position;
            p1 = path[i].position;
            p2 = path[i + 1].position;
            p3 = path[i + 2].position;

            segLen = Vector3.Distance(p1, p2);
            if (segLen < 0.0001f) segLen = 0.0001f;
        }

        Vector3 pos = CatmullRom(p0, p1, p2, p3, t);
        Vector3 tan = CatmullRomTangent(p0, p1, p2, p3, t);
        if (tan.sqrMagnitude < 0.000001f) tan = (p2 - p1);

        Vector3 desiredForward = tan.normalized;

        // Move + rotate car
        transform.position = pos;

        Quaternion desiredRot = Quaternion.LookRotation(desiredForward, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, rotationLerp * Time.deltaTime);

        // Steering wheel from car yaw rate (smoothed + limited speed)
        UpdateSteeringWheelFromYawRate();
    }

    private void UpdateSteeringWheelFromYawRate()
    {
        if (steeringWheel == null) return;

        float dt = Time.deltaTime;
        if (dt <= 0.000001f) return;

        // 1) RAW yaw rate
        float yaw = NormalizeAngle(transform.eulerAngles.y);
        float yawDelta = Mathf.DeltaAngle(lastYaw, yaw);    // signed deg this frame
        lastYaw = yaw;

        float rawYawRate = yawDelta / dt;                  // deg/sec

        // 2) Low-pass filter yaw rate (kills spikes)
        float st = Mathf.Max(0.0001f, yawRateSmoothTime);
        smoothedYawRate = Mathf.SmoothDamp(smoothedYawRate, rawYawRate, ref yawRateVel, st, Mathf.Infinity, dt);

        // 3) Map yawRate -> wheel target angle
        float denom = Mathf.Max(1f, yawRateForFullLock);
        float normalized = Mathf.Clamp(smoothedYawRate / denom, -1f, 1f);
        float targetWheel = normalized * maxWheelAngle * steeringGain;

        // 4) Limit how fast the wheel can rotate (deg/sec)
        float maxStep = Mathf.Max(1f, maxWheelSpeed) * dt;
        currentWheelAngle = Mathf.MoveTowards(currentWheelAngle, targetWheel, maxStep);

        float z = invertWheel ? -currentWheelAngle : currentWheelAngle;

        // Apply ONLY local Z
        Vector3 e = steeringWheel.localEulerAngles;
        e.z = wheelBaseLocalZ + z;
        steeringWheel.localEulerAngles = e;
    }

    private static float NormalizeAngle(float a)
    {
        if (a > 180f) a -= 360f;
        return a;
    }

    static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        return 0.5f * (
            (2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3
        );
    }

    static Vector3 CatmullRomTangent(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;

        return 0.5f * (
            (-p0 + p2) +
            2f * (2f * p0 - 5f * p1 + 4f * p2 - p3) * t +
            3f * (-p0 + 3f * p1 - 3f * p2 + p3) * t2
        );
    }
}
