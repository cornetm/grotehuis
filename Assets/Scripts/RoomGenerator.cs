using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoomGenerator : MonoBehaviour
{
    [Header("Room Spawn Settings")]
    public int MinRooms = 5;
    public int MaxRooms = 10;
    public float StartCooldown = 1f;

    [Header("Room Prefabs")]
    public GameObject[] Rooms;

    [Header("End Room Prefab")]
    public GameObject EndRoomPrefab;

    [Header("Monster Settings (X monsters per X kamers)")]
    public int MonstersPerRooms = 1;
    public int RoomsPerMonster = 10;

    private Transform RoomParent;
    private Transform MonsterParent;
    private List<GameObject> placedRooms = new List<GameObject>();

    private RoomInfo.RoomDirection? lastRoomDirection = null;
    private int leftStreak = 0;

    void Start()
    {
        SetupParents();
        Invoke(nameof(StartDungeonGeneration), StartCooldown);
    }

    void SetupParents()
    {
        GameObject roomObj = GameObject.Find("Rooms");
        if (roomObj == null) roomObj = new GameObject("Rooms");
        RoomParent = roomObj.transform;

        GameObject monsterObj = GameObject.Find("Monsters");
        if (monsterObj == null) monsterObj = new GameObject("Monsters");
        MonsterParent = monsterObj.transform;
    }

    void StartDungeonGeneration()
    {
        int targetRooms = Random.Range(MinRooms, MaxRooms + 1);
        bool success = GenerateDungeonRecursive(targetRooms, 0);

        if (success)
        {
            // Plaats de eindkamer als laatste
            PlaceEndRoom();

            Debug.Log($"Dungeon succesvol gegenereerd met {placedRooms.Count} kamers!");
            SpawnMonsters();
        }
        else
        {
            Debug.LogError("Dungeon generatie mislukt!");
        }
    }

    void PlaceEndRoom()
    {
        if (EndRoomPrefab == null) return;

        GameObject lastRoom = placedRooms[placedRooms.Count - 1];
        RoomInfo lastInfo = lastRoom.GetComponent<RoomInfo>();

        GameObject endRoom = Instantiate(EndRoomPrefab, Vector3.zero, Quaternion.identity, RoomParent);
        RoomInfo endInfo = endRoom.GetComponent<RoomInfo>();
        PlaceRoom(endRoom, endInfo);

        placedRooms.Add(endRoom);

        // Voeg trigger component toe om te detecteren dat speler gewonnen heeft
        EndRoomTrigger trigger = endRoom.AddComponent<EndRoomTrigger>();
        trigger.RoomGenerator = this;
    }

    public void PlayerReachedEnd()
    {
        Debug.Log("Je hebt de eindkamer bereikt! Je hebt gewonnen!");
        // Hier kun je scene switch, UI of win state triggeren
    }

    bool GenerateDungeonRecursive(int targetRooms, int currentIndex)
    {
        if (currentIndex == 0)
        {
            GameObject forwardPrefab = null;
            foreach (GameObject prefab in Rooms)
            {
                RoomInfo info = prefab.GetComponent<RoomInfo>();
                if (info != null && info.roomdirection == RoomInfo.RoomDirection.forward)
                {
                    forwardPrefab = prefab;
                    break;
                }
            }

            if (forwardPrefab == null)
            {
                Debug.LogError("Geen Forward prefab gevonden!");
                return false;
            }

            GameObject firstRoom = Instantiate(forwardPrefab, Vector3.zero, Quaternion.identity, RoomParent);
            RoomInfo firstInfo = firstRoom.GetComponent<RoomInfo>();
            PlaceRoom(firstRoom, firstInfo);
            placedRooms.Add(firstRoom);

            lastRoomDirection = firstInfo.roomdirection;
            leftStreak = 0;

            return GenerateDungeonRecursive(targetRooms, currentIndex + 1);
        }

        if (currentIndex >= targetRooms) return true;

        List<GameObject> availablePrefabs = new List<GameObject>(Rooms);
        ShuffleList(availablePrefabs);

        foreach (GameObject prefab in availablePrefabs)
        {
            GameObject newRoom = Instantiate(prefab, Vector3.zero, Quaternion.identity, RoomParent);
            RoomInfo info = newRoom.GetComponent<RoomInfo>();
            if (info == null || info.StartPoint == null || info.EndPoint == null)
            {
                Destroy(newRoom);
                continue;
            }

            if (LastRoomWasVertical() &&
                (info.roomdirection == RoomInfo.RoomDirection.upstairs || info.roomdirection == RoomInfo.RoomDirection.downstairs))
            {
                Destroy(newRoom);
                continue;
            }

            if (info.roomdirection == RoomInfo.RoomDirection.left && leftStreak >= 2)
            {
                if (!(lastRoomDirection == RoomInfo.RoomDirection.upstairs || lastRoomDirection == RoomInfo.RoomDirection.downstairs))
                {
                    Destroy(newRoom);
                    continue;
                }
            }

            PlaceRoom(newRoom, info);

            if (RoomOverlaps(newRoom))
            {
                Destroy(newRoom);
                continue;
            }

            if (placedRooms.Count > 0)
            {
                RoomInfo prev = placedRooms[placedRooms.Count - 1].GetComponent<RoomInfo>();
                if (Vector3.Distance(prev.EndPoint.position, info.StartPoint.position) > 0.01f)
                {
                    Destroy(newRoom);
                    continue;
                }
            }

            placedRooms.Add(newRoom);
            lastRoomDirection = info.roomdirection;
            leftStreak = (info.roomdirection == RoomInfo.RoomDirection.left) ? leftStreak + 1 : 0;

            bool result = GenerateDungeonRecursive(targetRooms, currentIndex + 1);
            if (result) return true;

            placedRooms.RemoveAt(placedRooms.Count - 1);
            Destroy(newRoom);
            RecalculateGeneratorState();
        }

        return false;
    }

    void SpawnMonsters()
    {
        List<MonsterSpawner> spawnerList = new List<MonsterSpawner>();

        // Voeg alleen spawners van kamers vanaf index 5 toe
        for (int i = 5; i < placedRooms.Count; i++)
        {
            MonsterSpawner spawner = placedRooms[i].GetComponentInChildren<MonsterSpawner>();
            if (spawner != null)
                spawnerList.Add(spawner);
        }

        ShuffleList(spawnerList);

        int minMonsters = Mathf.CeilToInt((float)placedRooms.Count / RoomsPerMonster) * MonstersPerRooms;
        int spawned = 0;

        List<int> usedIndices = new List<int>();

        // Force spawn minimaal aantal monsters zonder dat ze naast elkaar staan
        for (int i = 0; i < spawnerList.Count && spawned < minMonsters; i++)
        {
            if (IsAdjacentUsed(i, usedIndices)) continue;

            spawnerList[i].SetParent(MonsterParent);
            spawnerList[i].TrySpawn(MonsterParent, true);
            usedIndices.Add(i);
            spawned++;
        }

        // Extra monsters verspreid random, nog steeds niet naast elkaar
        for (int i = 0; i < spawnerList.Count; i++)
        {
            if (spawned >= spawnerList.Count) break;

            if (usedIndices.Contains(i) || IsAdjacentUsed(i, usedIndices)) continue;

            spawnerList[i].TrySpawn(MonsterParent, false);
            usedIndices.Add(i);
            spawned++;
        }

        Debug.Log($"Monsters toegewezen: {spawned}/{spawnerList.Count} kamers (minimaal {minMonsters})");
    }

    private bool IsAdjacentUsed(int index, List<int> usedIndices)
    {
        return usedIndices.Contains(index - 1) || usedIndices.Contains(index + 1);
    }

    void PlaceRoom(GameObject room, RoomInfo info)
    {
        Vector3 startOffset = info.StartPoint.localPosition;
        Transform lastEnd = (placedRooms.Count > 0) ? placedRooms[placedRooms.Count - 1].GetComponent<RoomInfo>().EndPoint : null;

        if (lastEnd == null)
        {
            room.transform.position = transform.position - startOffset;
            room.transform.rotation = Quaternion.identity;
            return;
        }

        Vector3 forwardDir = lastEnd.forward;
        room.transform.rotation = (info.roomdirection == RoomInfo.RoomDirection.upstairs || info.roomdirection == RoomInfo.RoomDirection.downstairs)
            ? Quaternion.LookRotation(forwardDir, Vector3.up)
            : lastEnd.rotation;

        Vector3 worldOffset = room.transform.TransformPoint(startOffset) - room.transform.position;
        room.transform.position = lastEnd.position - worldOffset;
    }

    bool RoomOverlaps(GameObject room)
    {
        Collider myCollider = room.GetComponent<Collider>();
        if (myCollider == null) return false;

        foreach (GameObject other in placedRooms)
        {
            if (other == null) continue;
            Collider otherCol = other.GetComponent<Collider>();
            if (otherCol == null) continue;

            Vector3 direction;
            float distance;
            if (Physics.ComputePenetration(
                myCollider, room.transform.position, room.transform.rotation,
                otherCol, other.transform.position, other.transform.rotation,
                out direction, out distance))
                return true;
        }
        return false;
    }

    void RecalculateGeneratorState()
    {
        if (placedRooms.Count == 0)
        {
            lastRoomDirection = null;
            leftStreak = 0;
        }
        else
        {
            RoomInfo lastInfo = placedRooms[placedRooms.Count - 1].GetComponent<RoomInfo>();
            lastRoomDirection = lastInfo.roomdirection;

            leftStreak = 0;
            for (int i = placedRooms.Count - 1; i >= 0; i--)
            {
                RoomInfo r = placedRooms[i].GetComponent<RoomInfo>();
                if (r.roomdirection == RoomInfo.RoomDirection.left) leftStreak++;
                else break;
            }
        }
    }

    bool LastRoomWasVertical()
    {
        if (placedRooms.Count == 0) return false;
        RoomInfo info = placedRooms[placedRooms.Count - 1].GetComponent<RoomInfo>();
        return info.roomdirection == RoomInfo.RoomDirection.upstairs || info.roomdirection == RoomInfo.RoomDirection.downstairs;
    }

    void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int rand = Random.Range(i, list.Count);
            list[i] = list[rand];
            list[rand] = temp;
        }
    }
}

/// <summary>
/// Trigger voor einde kamer.
/// </summary>
public class EndRoomTrigger : MonoBehaviour
{
    public RoomGenerator RoomGenerator;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            RoomGenerator.PlayerReachedEnd();
        }
    }
}