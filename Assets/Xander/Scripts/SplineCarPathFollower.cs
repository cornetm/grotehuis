using System.Collections.Generic;
using UnityEngine;

public class SplineCarPathFollower : MonoBehaviour
{
    [SerializeField] private RoadTileManager tileManager;

    [Header("Motion")]
    [SerializeField] private float speed = 12f;          // m/s langs de curve
    [SerializeField] private float rotationLerp = 10f;   // hoe snel hij tangent volgt

    [Header("Spline")]
    [Tooltip("Hoeveel meter per segment voordat we doorsteppen. 1.0 = t gaat 0..1 per segment op basis van afstand.")]
    [SerializeField] private float minPointsNeeded = 4;
    [SerializeField] private bool loopSafeIfQueueShrinks = true;

    // Waar we zijn op de polyline:
    private int i = 0;      // wijst naar p1 (segment start) in Catmull (p0,p1,p2,p3)
    private float t = 0f;   // 0..1 binnen segment p1->p2

    void Update()
    {
        if (tileManager == null) return;
        IReadOnlyList<Transform> path = tileManager.PathQueue;
        if (path == null || path.Count < 4) return;

        // Zorg dat i geldig blijft, ook als TileManager queue trimmed
        i = Mathf.Clamp(i, 1, path.Count - 3);

        // Pak control points
        Vector3 p0 = path[i - 1].position;
        Vector3 p1 = path[i].position;
        Vector3 p2 = path[i + 1].position;
        Vector3 p3 = path[i + 2].position;

        // Segment length approx (voor t advance). Voor beter kan je subdividen, maar dit is prima.
        float segLen = Vector3.Distance(p1, p2);
        if (segLen < 0.0001f) segLen = 0.0001f;

        // Advance t op basis van meters/sec
        float dt = (speed * Time.deltaTime) / segLen;
        t += dt;

        // Als we voorbij dit segment zijn, ga naar volgende
        while (t >= 1f)
        {
            t -= 1f;
            i++;

            // Als we te dicht bij einde zitten, clamp (tileManager zal ondertussen nieuwe waypoints toevoegen)
            i = Mathf.Clamp(i, 1, path.Count - 3);

            // Optional: consume oude punten zodat queue klein blijft
            // We consumeren alleen als we al een paar punten achter ons hebben.
            if (i > 10)
            {
                int consume = i - 5;
                tileManager.ConsumeWaypointsUpTo(consume);
                i -= consume;
            }

            // refresh control points na consume/shift
            p0 = path[i - 1].position;
            p1 = path[i].position;
            p2 = path[i + 1].position;
            p3 = path[i + 2].position;

            segLen = Vector3.Distance(p1, p2);
            if (segLen < 0.0001f) segLen = 0.0001f;
        }

        // Sample positie op spline
        Vector3 pos = CatmullRom(p0, p1, p2, p3, t);

        // Sample tangent (richting) -> derivative
        Vector3 tan = CatmullRomTangent(p0, p1, p2, p3, t);
        if (tan.sqrMagnitude < 0.000001f) tan = (p2 - p1);

        // Apply
        transform.position = pos;

        Quaternion desiredRot = Quaternion.LookRotation(tan.normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, rotationLerp * Time.deltaTime);
    }

    // Catmull-Rom spline (centripetal variant is nog mooier, maar dit is al huge upgrade)
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

        // derivative van bovenstaande
        return 0.5f * (
            (-p0 + p2) +
            2f * (2f * p0 - 5f * p1 + 4f * p2 - p3) * t +
            3f * (-p0 + 3f * p1 - 3f * p2 + p3) * t2
        );
    }
}
