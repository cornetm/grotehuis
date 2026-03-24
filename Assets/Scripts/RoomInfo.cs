using UnityEngine;
using System.Collections.Generic;

public class RoomInfo : MonoBehaviour
{
    [Header("Room Info")]
    public RoomDirection roomdirection;

    public Transform StartPoint;
    public Transform EndPoint;

    // ✅ NIEUW: lights lijst
    [Header("Lights")]
    public List<Light> RoomLights = new List<Light>();

    // ✅ NIEUW: kans per kamer
    [Range(0, 100)] public float LightChance = 70f;

    public enum RoomDirection
    {
        left,
        right,
        forward,
        upstairs,
        downstairs
    }
}