using UnityEngine;
using System.Collections;
using JetBrains.Annotations;

public class RoomGenerator : MonoBehaviour
{
    [Header("RoomSpawn")]
    public int RoomCount;
    public int MaxRooms;
    public float StartCooldown;

    [Header("Rooms")]
    public GameObject[] Rooms;

    private Transform RoomParent; // 🔹 Parent reference
    void Start()
    {
        SetupParent();
        StartCoroutine(StartRoomSpawn());
    }
    private void SetupParent()
    {
        GameObject parentObj = GameObject.Find("Room");

        if (parentObj == null)
        {
            parentObj = new GameObject("Room");
        }

        RoomParent = parentObj.transform;
    }
    IEnumerator StartRoomSpawn()
    {
        yield return new WaitForSeconds(StartCooldown); // wacht eerst
        RoomSpawn(); // daarna uitvoeren
    }

    public void RoomSpawn()
    {
        for (int i = 0; i < MaxRooms; i++) SpawnRoom();
    }

    public void SpawnRoom()
    {
        RoomCount += 1;

        int index = Random.Range(0, Rooms.Length);
        GameObject prefab = Rooms[index];
    }
}