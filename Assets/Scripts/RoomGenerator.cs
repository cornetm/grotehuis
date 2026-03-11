using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoomGenerator : MonoBehaviour
{
    [Header("Room Spawn Settings")]
    public int MinRooms = 5;
    public int MaxRooms = 10;
    public float StartCooldown = 1f;
    public int MaxTryPerRoom = 15;

    [Header("Rooms Prefabs")]
    public GameObject[] Rooms;

    private Transform RoomParent;
    private Transform lastEndPoint;

    private List<GameObject> placedRooms = new List<GameObject>();

    private RoomInfo.RoomDirection? lastRoomDirection = null;
    private int leftStreak = 0;

    void Start()
    {
        SetupParent();
        StartCoroutine(StartRoomSpawn());
    }

    void SetupParent()
    {
        GameObject parentObj = GameObject.Find("Rooms");

        if (parentObj == null)
            parentObj = new GameObject("Rooms");

        RoomParent = parentObj.transform;
    }

    IEnumerator StartRoomSpawn()
    {
        yield return new WaitForSeconds(StartCooldown);
        RoomSpawn();
    }

    void RoomSpawn()
    {
        int targetRooms = Random.Range(MinRooms, MaxRooms + 1);

        int safety = 0;

        while (placedRooms.Count < targetRooms && safety < 500)
        {
            safety++;

            if (!TryPlaceRoom())
            {
                if (placedRooms.Count == 0)
                    break;

                GameObject last = placedRooms[placedRooms.Count - 1];

                Debug.Log("BACKTRACK: verwijderen kamer -> " + last.name);

                placedRooms.RemoveAt(placedRooms.Count - 1);
                Destroy(last);

                RecalculateGeneratorState();
            }
        }

        Debug.Log("Dungeon generatie klaar. Rooms geplaatst: " + placedRooms.Count);
    }

    bool TryPlaceRoom()
    {
        for (int attempt = 0; attempt < MaxTryPerRoom; attempt++)
        {
            GameObject prefab = Rooms[Random.Range(0, Rooms.Length)];

            GameObject newRoom = Instantiate(prefab, Vector3.zero, Quaternion.identity, RoomParent);

            RoomInfo info = newRoom.GetComponent<RoomInfo>();

            if (info == null || info.StartPoint == null || info.EndPoint == null)
            {
                Debug.LogError("Room mist Start of End point");
                Destroy(newRoom);
                continue;
            }

            // upstairs/downstairs regel
            if (lastRoomDirection.HasValue)
            {
                if ((lastRoomDirection == RoomInfo.RoomDirection.upstairs && info.roomdirection == RoomInfo.RoomDirection.downstairs) ||
                    (lastRoomDirection == RoomInfo.RoomDirection.downstairs && info.roomdirection == RoomInfo.RoomDirection.upstairs))
                {
                    Destroy(newRoom);
                    continue;
                }
            }

            // max 3x left
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
                Debug.Log("Overlap -> verwijderen: " + newRoom.name);
                Destroy(newRoom);
                continue;
            }

            placedRooms.Add(newRoom);

            lastEndPoint = info.EndPoint;
            lastRoomDirection = info.roomdirection;

            transform.position = lastEndPoint.position;
            transform.rotation = lastEndPoint.rotation;

            if (info.roomdirection == RoomInfo.RoomDirection.left)
                leftStreak++;
            else
                leftStreak = 0;

            Debug.Log("Room geplaatst -> " + newRoom.name);

            return true;
        }

        Debug.Log("Geen kamer past hier.");
        return false;
    }

    void PlaceRoom(GameObject room, RoomInfo info)
    {
        Vector3 startOffset = info.StartPoint.localPosition;

        if (lastEndPoint == null)
        {
            room.transform.position = transform.position - startOffset;
            room.transform.rotation = Quaternion.identity;
        }
        else
        {
            room.transform.rotation = lastEndPoint.rotation;

            Vector3 worldOffset = room.transform.TransformPoint(startOffset) - room.transform.position;

            room.transform.position = lastEndPoint.position - worldOffset;
        }
    }

    bool RoomOverlaps(GameObject room)
    {
        Collider myCollider = room.GetComponent<Collider>();

        if (myCollider == null)
            return false;

        foreach (GameObject other in placedRooms)
        {
            if (other == null)
                continue;

            Collider otherCol = other.GetComponent<Collider>();

            if (otherCol == null)
                continue;

            if (placedRooms.Count >= 2)
            {
                if (other == placedRooms[placedRooms.Count - 1] ||
                    other == placedRooms[placedRooms.Count - 2])
                    continue;
            }

            Vector3 direction;
            float distance;

            bool overlap = Physics.ComputePenetration(
                myCollider, room.transform.position, room.transform.rotation,
                otherCol, other.transform.position, other.transform.rotation,
                out direction, out distance
            );

            if (overlap)
            {
                Debug.Log("Trigger overlap tussen " + room.name + " en " + other.name);
                return true;
            }
        }

        return false;
    }

    void RecalculateGeneratorState()
    {
        if (placedRooms.Count == 0)
        {
            lastEndPoint = null;
            lastRoomDirection = null;
            leftStreak = 0;
            return;
        }

        GameObject lastRoom = placedRooms[placedRooms.Count - 1];
        RoomInfo info = lastRoom.GetComponent<RoomInfo>();

        lastEndPoint = info.EndPoint;
        lastRoomDirection = info.roomdirection;

        transform.position = lastEndPoint.position;
        transform.rotation = lastEndPoint.rotation;

        leftStreak = 0;

        for (int i = placedRooms.Count - 1; i >= 0; i--)
        {
            RoomInfo r = placedRooms[i].GetComponent<RoomInfo>();

            if (r.roomdirection == RoomInfo.RoomDirection.left)
                leftStreak++;
            else
                break;
        }

        Debug.Log("Generator state opnieuw berekend. Nieuwe laatste kamer: " + lastRoom.name);
    }
}