using Unity.Cinemachine;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class RoadTileManager : MonoBehaviour
{
    [Header("Tile Prefabs")]
    [SerializeField] private RoadTile loopTilePrefab;
    [Header("Crash Tile (existing scene object, no clone)")]
    [SerializeField] private RoadTile crashTile;

    [Header("Setup")]
    [SerializeField] private int initialTiles = 4;
    [SerializeField] private Transform tilesParent;

    [Header("Recycling")]
    [SerializeField] private Transform car;
    [SerializeField] private float recycleBehindDistance = 60f;

    [Header("Crash Sequence")]
    [SerializeField] private SplineCarPathFollower splineCarPathFollowerToDisable;
    [SerializeField] private float crashTriggerDistance = 0.01f;

    [Header("Enable On Crash Trigger")]
    [SerializeField] private Animation animationComponentToEnable;
    [SerializeField] private PlayableDirector playableDirectorToEnable;
    [SerializeField] private CinemachineSplineCart cinemachineSplineCartToEnable;
    [SerializeField] private CinemachineCrashShake cinemachineCrashShakeToEnable;
    [SerializeField] private SignalReceiver signalReceiverToEnable;

    [Header("Activate On Crash Trigger")]
    [SerializeField] private GameObject activateOnCrashA;
    [SerializeField] private GameObject activateOnCrashB;

    [Header("Optional Auto Play")]
    [SerializeField] private bool playAnimationWhenTriggered = false;
    [SerializeField] private bool playDirectorWhenTriggered = true;

    private readonly List<RoadTile> tiles = new List<RoadTile>();
    private readonly List<Transform> pathQueue = new List<Transform>();

    public IReadOnlyList<Transform> PathQueue => pathQueue;

    private bool carCrashSequenceStarted;
    private bool recyclingStopped;
    private bool crashTileSpawned;
    private bool crashTriggered;

    private RoadTile spawnedCrashTile;
    private Transform crashActivationPoint;

    void Start()
    {
        if (tilesParent == null) tilesParent = this.transform;

        if (crashTile != null)
        {
            crashTile.AutoFind();
            crashTile.gameObject.SetActive(false);
        }

        RoadTile first = Instantiate(loopTilePrefab, tilesParent);
        first.AutoFind();
        tiles.Add(first);

        AppendTileWaypoints(first, skipFirst: false);

        for (int i = 1; i < initialTiles; i++)
        {
            SpawnAndAttachLoopTile();
        }
    }

    void Update()
    {
        if (car == null || tiles.Count == 0) return;

        if (!recyclingStopped)
        {
            RoadTile oldest = tiles[0];

            if (Vector3.Distance(car.position, oldest.exit.position) > recycleBehindDistance
                && IsTileBehindCar(oldest))
            {
                RecycleOldestToFront();
            }
        }

        if (carCrashSequenceStarted && crashTileSpawned && !crashTriggered && crashActivationPoint != null)
        {
            Vector3 toCar = car.position - crashActivationPoint.position;
            float passed = Vector3.Dot(crashActivationPoint.forward, toCar);

            if (passed >= 0f)
            {
                TriggerCrashIntroNow();
            }
        }
    }

    public void StartCarCrashSequence()
    {
        Debug.Log("[RoadTileManager] StartCarCrashSequence called.");

        if (carCrashSequenceStarted)
        {
            Debug.Log("[RoadTileManager] Crash sequence was already started, ignoring.");
            return;
        }

        carCrashSequenceStarted = true;
        recyclingStopped = true;

        if (crashTile == null)
        {
            Debug.LogWarning("[RoadTileManager] crashTile is not assigned.");
            return;
        }

        Debug.Log("[RoadTileManager] Recycling stopped. Spawning crash tile.");
        SpawnCrashTileAtEnd();
    }

    bool IsTileBehindCar(RoadTile tile)
    {
        Vector3 toExit = tile.exit.position - car.position;
        return Vector3.Dot(car.forward, toExit) < 0f;
    }

    void SpawnAndAttachLoopTile()
    {
        RoadTile newTile = Instantiate(loopTilePrefab, tilesParent);
        newTile.AutoFind();

        AttachTile(tiles[tiles.Count - 1], newTile);
        tiles.Add(newTile);

        AppendTileWaypoints(newTile, skipFirst: true);
    }

    void RecycleOldestToFront()
    {
        RoadTile tile = tiles[0];
        tiles.RemoveAt(0);

        RoadTile last = tiles[tiles.Count - 1];
        AttachTile(last, tile);

        tiles.Add(tile);

        AppendTileWaypoints(tile, skipFirst: true);
    }

    void SpawnCrashTileAtEnd()
    {
        if (crashTileSpawned)
        {
            Debug.Log("[RoadTileManager] Crash tile already positioned.");
            return;
        }

        spawnedCrashTile = crashTile;

        if (spawnedCrashTile == null)
        {
            Debug.LogWarning("[RoadTileManager] crashTile is null.");
            return;
        }

        spawnedCrashTile.AutoFind();

        if (spawnedCrashTile.transform.parent != tilesParent)
            spawnedCrashTile.transform.SetParent(tilesParent, true);

        spawnedCrashTile.gameObject.SetActive(true);

        RoadTile last = tiles[tiles.Count - 1];
        AttachTile(last, spawnedCrashTile);
        tiles.Add(spawnedCrashTile);

        // crash tile moet WP_0 WEL meenemen
        AppendTileWaypoints(spawnedCrashTile, skipFirst: false);

        if (spawnedCrashTile.waypoints != null && spawnedCrashTile.waypoints.Length > 0)
        {
            crashActivationPoint = spawnedCrashTile.waypoints[0];
            Debug.Log($"[RoadTileManager] Existing crash tile positioned. Activation point = waypoint[0] ({crashActivationPoint.name})");
        }
        else
        {
            crashActivationPoint = spawnedCrashTile.entry;
            Debug.Log($"[RoadTileManager] Existing crash tile positioned. Activation point fallback = entry ({crashActivationPoint.name})");
        }

        if (splineCarPathFollowerToDisable != null && crashActivationPoint != null)
        {
            splineCarPathFollowerToDisable.SetRotationAssist(crashActivationPoint, 6f);
        }

        crashTileSpawned = true;

        Debug.Log($"[RoadTileManager] PathQueue count after crash tile append: {pathQueue.Count}");
    }

    void TriggerCrashIntroNow()
    {
        if (crashTriggered)
            return;

        Debug.Log("[RoadTileManager] Crash activation point reached -> triggering crash intro now.");

        crashTriggered = true;

        if (splineCarPathFollowerToDisable != null)
            splineCarPathFollowerToDisable.enabled = false;

        if (animationComponentToEnable != null)
        {
            animationComponentToEnable.enabled = true;

            if (playAnimationWhenTriggered)
                animationComponentToEnable.Play();
        }

        if (signalReceiverToEnable != null)
            signalReceiverToEnable.enabled = true;

        if (cinemachineSplineCartToEnable != null)
            cinemachineSplineCartToEnable.enabled = true;

        if (cinemachineCrashShakeToEnable != null)
            cinemachineCrashShakeToEnable.enabled = true;

        if (playableDirectorToEnable != null)
        {
            playableDirectorToEnable.enabled = true;

            if (playDirectorWhenTriggered)
                playableDirectorToEnable.Play();
        }

        if (activateOnCrashA != null)
            activateOnCrashA.SetActive(true);

        if (activateOnCrashB != null)
            activateOnCrashB.SetActive(true);
    }

    void AttachTile(RoadTile prev, RoadTile next)
    {
        Transform nextRoot = next.transform;
        Transform nextEntry = next.entry;

        Vector3 entryLocalPos = nextRoot.InverseTransformPoint(nextEntry.position);

        Quaternion targetRot = Quaternion.LookRotation(prev.exit.forward, prev.exit.up);
        Quaternion entryLocalRot = Quaternion.Inverse(nextRoot.rotation) * nextEntry.rotation;
        nextRoot.rotation = targetRot * Quaternion.Inverse(entryLocalRot);

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

    public void ConsumeWaypointsUpTo(int globalIndex)
    {
        int removeCount = Mathf.Clamp(globalIndex, 0, pathQueue.Count);
        if (removeCount > 0)
            pathQueue.RemoveRange(0, removeCount);
    }
}