using UnityEngine;

public class OpenDOor : MonoBehaviour
{
    [Header("Door")]
    public DoorVariant doorvariant;

    public enum DoorVariant
    {
        DoorLeft,
        DoorRight,
        Lade,
    }

    [Header("State")]
    public bool isOpen = false;

    [Header("Settings")]
    public float openAngle = 90f;
    public float openDistance = 0.5f;
    public float smoothSpeed = 6f;

    private Quaternion closedRot;
    private Quaternion openRot;

    private Vector3 closedPos;
    private Vector3 openPos;

    void Start()
    {
        closedRot = transform.localRotation;
        closedPos = transform.localPosition;

        SetupTargets();
    }

    void SetupTargets()
    {
        switch (doorvariant)
        {
            case DoorVariant.DoorLeft:
                openRot = closedRot * Quaternion.Euler(0f, openAngle, 0f);
                break;

            case DoorVariant.DoorRight:
                openRot = closedRot * Quaternion.Euler(0f, -openAngle, 0f);
                break;

            case DoorVariant.Lade:
                // ✅ FIX: alleen Z-as beweging (voor/achter)
                openPos = closedPos + transform.localRotation * Vector3.forward * openDistance;
                break;
        }
    }

    public void ToggleDoor()
    {
        isOpen = !isOpen;
    }

    void Update()
    {
        if (doorvariant == DoorVariant.Lade)
        {
            Vector3 targetPos = isOpen ? openPos : closedPos;

            transform.localPosition =
                Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * smoothSpeed);
        }
        else
        {
            Quaternion targetRot = isOpen ? openRot : closedRot;

            transform.localRotation =
                Quaternion.Slerp(transform.localRotation, targetRot, Time.deltaTime * smoothSpeed);
        }
    }
}