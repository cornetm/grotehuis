using UnityEngine;

public class RoomInfo : MonoBehaviour
{

    [Header("Roominfo")]
    public RoomDirection roomdirection;

    public enum RoomDirection
     {
        left,
        right,
        forward,
        upstairs,
        downstairs
    }
}
