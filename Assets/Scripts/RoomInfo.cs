using UnityEngine;

public class RoomInfo : MonoBehaviour
{
    [Header("Room Info")]
    public RoomDirection roomdirection;

    public Transform StartPoint;
    public Transform EndPoint;

    public enum RoomDirection
    {
        left,
        right,
        forward,
        upstairs,
        downstairs
    }
}