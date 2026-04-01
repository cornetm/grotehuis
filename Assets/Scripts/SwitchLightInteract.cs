using UnityEngine;

public class SwitchLightInteract : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationAngle = 180f; // Rotatie bij aan/uit
    private bool isOn = false;

    private Quaternion offRotation;
    private Quaternion onRotation;

    // 🔥 NIEUW: Audio voor switch
    [Header("Switch Audio")]
    public AudioSource switchSound;

    void Start()
    {
        // Beginpositie
        offRotation = transform.localRotation;
        onRotation = offRotation * Quaternion.Euler(0f, 0f, rotationAngle);
    }

    void Update()
    {
        if (IsPlayerLookingAtSwitch() && Input.GetKeyDown(KeyCode.E))
        {
            ToggleSwitch();
        }
    }

    void ToggleSwitch()
    {
        isOn = !isOn;

        // Draai onmiddellijk
        transform.localRotation = isOn ? onRotation : offRotation;

        // 🔥 Speel geluid bij schakelen
        if (switchSound != null)
            switchSound.Play();
    }

    bool IsPlayerLookingAtSwitch()
    {
        Camera cam = Camera.main;
        if (cam == null) return false;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 5f))
        {
            return hit.collider.gameObject == gameObject;
        }

        return false;
    }
}