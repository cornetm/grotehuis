using UnityEngine;

public class RoomInfo : MonoBehaviour
{
    [Header("Room Direction")]
    public RoomDirection roomdirection;

    public enum RoomDirection
    {
        left,
        right,
        forward,
        upstairs,
        downstairs
    }

    [Header("Room Connectors")]
    public Transform StartPoint;
    public Transform EndPoint;
}