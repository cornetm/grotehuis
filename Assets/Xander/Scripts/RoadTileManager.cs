using System.Collections.Generic;
using UnityEngine;

public class RoadTileManager : MonoBehaviour
{
    [Header("Tile Prefabs")]
    [SerializeField] private RoadTile loopTilePrefab;          // jouw infinite tile
    // Later: [SerializeField] private RoadTile crashTilePrefab;

    [Header("Setup")]
    [SerializeField] private int initialTiles = 4;
    [SerializeField] private Transform tilesParent;

    [Header("Recycling")]
    [SerializeField] private Transform car;
    [SerializeField] private float recycleBehindDistance = 60f; // tile exit ver achter car => recycle

    // Actieve tiles in volgorde
    private readonly List<RoadTile> tiles = new List<RoadTile>();

    // Globale waypoint queue (Transform refs)
    private readonly List<Transform> pathQueue = new List<Transform>();

    public IReadOnlyList<Transform> PathQueue => pathQueue;

    void Start()
    {
        if (tilesParent == null) tilesParent = this.transform;

        // Spawn eerste tile op zijn huidige plek (zoals jij 'm in scene wil)
        RoadTile first = Instantiate(loopTilePrefab, tilesParent);
        first.AutoFind();
        tiles.Add(first);

        // Voeg waypoints van tile toe aan queue
        AppendTileWaypoints(first, skipFirst: false);

        // Spawn extra tiles aan elkaar vast
        for (int i = 1; i < initialTiles; i++)
        {
            SpawnAndAttachLoopTile();
        }
    }

    void Update()
    {
        if (car == null || tiles.Count == 0) return;

        // Recycle tiles die ver achter de auto liggen
        RoadTile oldest = tiles[0];
        float behind = car.position.z - oldest.exit.position.z;
        // ^ Let op: dit werkt als "vooruit" ongeveer +Z is. 
        // Als jouw wereld andere richting is, doen we dit op afstand i.p.v. z.

        if (Vector3.Distance(car.position, oldest.exit.position) > recycleBehindDistance
            && IsTileBehindCar(oldest))
        {
            RecycleOldestToFront();
        }
    }

    bool IsTileBehindCar(RoadTile tile)
    {
        // Algemene check: is exit "achter" de auto gezien auto forward?
        Vector3 toExit = tile.exit.position - car.position;
        return Vector3.Dot(car.forward, toExit) < 0f;
    }

    void SpawnAndAttachLoopTile()
    {
        RoadTile newTile = Instantiate(loopTilePrefab, tilesParent);
        newTile.AutoFind();

        AttachTile(tiles[tiles.Count - 1], newTile);
        tiles.Add(newTile);

        // Bij het aanplakken wil je geen dubbele seam-waypoint (WP_0==Exit van vorige)
        AppendTileWaypoints(newTile, skipFirst: true);
    }

    void RecycleOldestToFront()
    {
        // pak oudste tile, haal 'm uit lijst
        RoadTile tile = tiles[0];
        tiles.RemoveAt(0);

        // attach achter de huidige laatste tile
        RoadTile last = tiles[tiles.Count - 1];
        AttachTile(last, tile);

        // tile weer achteraan zetten
        tiles.Add(tile);

        // add waypoints van deze tile opnieuw aan queue (skip first seam)
        AppendTileWaypoints(tile, skipFirst: true);
    }

    void AttachTile(RoadTile prev, RoadTile next)
    {
        // We willen next.entry exact matchen met prev.exit (positie + rotatie)
        // 1) bereken delta tussen nextRoot en nextEntry
        Transform nextRoot = next.transform;
        Transform nextEntry = next.entry;

        // offset van root naar entry in lokale/wereld ruimte
        Vector3 entryLocalPos = nextRoot.InverseTransformPoint(nextEntry.position);

        // 2) Zet rotatie zodat entry forward/up matcht met prev.exit forward/up
        Quaternion rotDelta = Quaternion.FromToRotation(nextEntry.forward, prev.exit.forward);
        // Voor betere matching ook up-vector:
        Quaternion targetRot = Quaternion.LookRotation(prev.exit.forward, prev.exit.up);
        // We zetten root rot = targetRot * inverse(entryLocalRot)
        Quaternion entryLocalRot = Quaternion.Inverse(nextRoot.rotation) * nextEntry.rotation;
        nextRoot.rotation = targetRot * Quaternion.Inverse(entryLocalRot);

        // 3) Zet positie zodat entry pos == prev.exit pos
        Vector3 newEntryWorldPosAfterRot = nextRoot.TransformPoint(entryLocalPos);
        nextRoot.position += (prev.exit.position - newEntryWorldPosAfterRot);
    }

    void AppendTileWaypoints(RoadTile tile, bool skipFirst)
    {
        if (tile.waypoints == null || tile.waypoints.Length == 0) return;

        int start = skipFirst ? 1 : 0;
        for (int i = start; i < tile.waypoints.Length; i++)
            pathQueue.Add(tile.waypoints[i]);
    }

    /// CarFollower roept dit aan om te zeggen: "ik heb waypoint index X gehaald"
    public void ConsumeWaypointsUpTo(int globalIndex)
    {
        // Verwijder alles vóór (en incl) globalIndex, zodat queue niet oneindig groeit
        // globalIndex is index in de huidige queue.
        int removeCount = Mathf.Clamp(globalIndex, 0, pathQueue.Count);
        if (removeCount > 0)
            pathQueue.RemoveRange(0, removeCount);
    }
}
