using UnityEngine;
using System.Collections.Generic;

public class RoomGenerator : MonoBehaviour
{
    [Header("Room Spawn Settings")]
    public int MinRooms = 5;
    public int MaxRooms = 10;
    public float StartCooldown = 1f;

    [Header("Room Prefabs")]
    public GameObject[] Rooms; // forward, left, right, upstairs, downstairs

    private Transform RoomParent;
    private List<GameObject> placedRooms = new List<GameObject>();
    private RoomInfo.RoomDirection? lastRoomDirection = null;
    private int leftStreak = 0;

    void Start()
    {
        SetupParent();
        Invoke(nameof(StartDungeonGeneration), StartCooldown);
    }

    void SetupParent()
    {
        GameObject parentObj = GameObject.Find("Rooms");
        if (parentObj == null)
            parentObj = new GameObject("Rooms");

        RoomParent = parentObj.transform;
    }

    void StartDungeonGeneration()
    {
        int targetRooms = Random.Range(MinRooms, MaxRooms + 1);
        bool success = GenerateDungeonRecursive(targetRooms, 0);

        if (success)
            Debug.Log($"Dungeon succesvol gegenereerd met {placedRooms.Count} kamers!");
        else
            Debug.LogError("Dungeon generatie mislukt! (Dit mag niet gebeuren als de kamers fysiek passen.)");
    }

    // Recursive backtracking generator
    bool GenerateDungeonRecursive(int targetRooms, int currentIndex)
    {
        // Eerste kamer altijd Forward
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
                Debug.LogError("Geen Forward prefab gevonden voor de eerste kamer!");
                return false;
            }

            GameObject firstRoom = Instantiate(forwardPrefab, Vector3.zero, Quaternion.identity, RoomParent);
            RoomInfo firstInfo = firstRoom.GetComponent<RoomInfo>();
            PlaceRoom(firstRoom, firstInfo);
            placedRooms.Add(firstRoom);
            lastRoomDirection = firstInfo.roomdirection;
            leftStreak = 0;

            // Recursive call voor de rest
            return GenerateDungeonRecursive(targetRooms, currentIndex + 1);
        }

        if (currentIndex >= targetRooms)
            return true; // Alle kamers geplaatst

        // Shuffle de prefab lijst zodat we elke prefab 1x proberen
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

            // Vertical room check: nooit 2x vertical achter elkaar
            if (LastRoomWasVertical() &&
                (info.roomdirection == RoomInfo.RoomDirection.upstairs || info.roomdirection == RoomInfo.RoomDirection.downstairs))
            {
                Destroy(newRoom);
                continue;
            }

            // Max 3x left check
            if (info.roomdirection == RoomInfo.RoomDirection.left && leftStreak >= 2)
            {
                if (!(lastRoomDirection == RoomInfo.RoomDirection.upstairs || lastRoomDirection == RoomInfo.RoomDirection.downstairs))
                {
                    Destroy(newRoom);
                    continue;
                }
            }

            // Plaats kamer correct
            PlaceRoom(newRoom, info);

            // Overlap check
            if (RoomOverlaps(newRoom))
            {
                Destroy(newRoom);
                continue;
            }

            // Chain check: start moet matchen met vorige end
            if (placedRooms.Count > 0)
            {
                RoomInfo prev = placedRooms[placedRooms.Count - 1].GetComponent<RoomInfo>();
                float dist = Vector3.Distance(prev.EndPoint.position, info.StartPoint.position);
                if (dist > 0.01f)
                {
                    Destroy(newRoom);
                    continue;
                }
            }

            // ✅ Plaats definitief
            placedRooms.Add(newRoom);
            lastRoomDirection = info.roomdirection;
            leftStreak = (info.roomdirection == RoomInfo.RoomDirection.left) ? leftStreak + 1 : 0;

            // Recursieve call: probeer volgende kamer
            bool result = GenerateDungeonRecursive(targetRooms, currentIndex + 1);
            if (result)
            {
                Debug.Log($"Room geplaatst: {newRoom.name} ({info.roomdirection})");
                return true;
            }
            else
            {
                // Backtrack: verwijder kamer en probeer volgende prefab
                placedRooms.RemoveAt(placedRooms.Count - 1);
                Destroy(newRoom);
                RecalculateGeneratorState();
                Debug.Log($"Backtrack: verwijderen kamer '{prefab.name}' op index {currentIndex}");
            }
        }

        // Geen prefab past → terug naar vorige kamer
        return false;
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

        if (info.roomdirection == RoomInfo.RoomDirection.upstairs || info.roomdirection == RoomInfo.RoomDirection.downstairs)
        {
            room.transform.rotation = Quaternion.LookRotation(forwardDir, Vector3.up);
        }
        else
        {
            room.transform.rotation = lastEnd.rotation;
        }

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
            {
                return true;
            }
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
                if (r.roomdirection == RoomInfo.RoomDirection.left)
                    leftStreak++;
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