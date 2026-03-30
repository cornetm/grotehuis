using UnityEngine;

public class OpenDOor : MonoBehaviour
{
    [Header("Door Type")]
    public DoorState doorStateType;

    public enum DoorState
    {
        NormalDoor,
        ClosedDoor
    }

    [Header("Door Variant")]
    public DoorVariant doorvariant;

    public enum DoorVariant
    {
        DoorLeft,
        DoorRight,
        Lade,
        Lucifer // 🔥 NIEUW
    }

    [Header("State")]
    public bool isOpen = false;

    [Header("Settings")]
    public float openAngle = 90f;
    public float openDistance = 0.5f;
    public float smoothSpeed = 6f;

    [Header("Handles (slepen in Inspector)")]
    public HandleGame handle1;
    public HandleGame handle2;
    public HandleGame handle3;

    [Header("Lampjes (slepen in Inspector)")]
    public Light lamp1;
    public Light lamp2;
    public Light lamp3;

    // 🔥 NIEUW
    [Header("Lucifer Light")]
    public Light luciferLight;

    private Quaternion closedRot;
    private Quaternion openRot;

    private Vector3 closedPos;
    private Vector3 openPos;

    // 🔥 NIEUW
    private int luciferState = 0;

    void Start()
    {
        closedRot = transform.localRotation;
        closedPos = transform.localPosition;

        SetupTargets();

        // 🔥 Licht standaard uit
        if (luciferLight != null)
            luciferLight.enabled = false;
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
            case DoorVariant.Lucifer: // 🔥 zelfde gedrag als lade
                openPos = closedPos + transform.localRotation * Vector3.forward * openDistance;
                break;
        }
    }

    void Update()
    {
        if (doorStateType == DoorState.ClosedDoor)
        {
            if (handle1 != null && lamp1 != null)
                lamp1.enabled = handle1.IsUsed();
            if (handle2 != null && lamp2 != null)
                lamp2.enabled = handle2.IsUsed();
            if (handle3 != null && lamp3 != null)
                lamp3.enabled = handle3.IsUsed();

            if (!isOpen)
            {
                bool allOn = true;

                if (lamp1 != null && !lamp1.enabled) allOn = false;
                if (lamp2 != null && !lamp2.enabled) allOn = false;
                if (lamp3 != null && !lamp3.enabled) allOn = false;

                if (allOn)
                    isOpen = true;
            }
        }

        // 🔥 Lucifer gebruikt lade beweging
        if (doorvariant == DoorVariant.Lade || doorvariant == DoorVariant.Lucifer)
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

    public void ToggleDoor()
    {
        // 🔥 LUCIFER LOGICA
        if (doorvariant == DoorVariant.Lucifer)
        {
            luciferState++;

            if (luciferState == 1)
            {
                // Open lade
                isOpen = true;
            }
            else if (luciferState == 2)
            {
                // Licht aan
                if (luciferLight != null)
                    luciferLight.enabled = true;
            }
            else if (luciferState == 3)
            {
                // Licht uit + lade dicht
                if (luciferLight != null)
                    luciferLight.enabled = false;

                isOpen = false;

                luciferState = 0; // reset cycle
            }

            return;
        }

        // 🔹 OUDE LOGICA (NIET AANGEPAST)
        if (doorStateType == DoorState.NormalDoor)
            isOpen = !isOpen;
    }
}