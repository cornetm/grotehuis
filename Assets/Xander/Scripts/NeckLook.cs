using UnityEngine;

public class NeckLook : MonoBehaviour
{
    [Header("Sensitivity")]
    public float sensitivityX = 120f;
    public float sensitivityY = 100f;

    [Header("Rotation Limits")]
    public float maxYaw = 70f;     // links/rechts
    public float maxPitchUp = 40f; // omhoog
    public float maxPitchDown = 30f; // omlaag

    [Header("Smoothing")]
    public float smoothTime = 8f;

    float yaw;
    float pitch;

    Quaternion targetRotation;

    void Start()
    {
        Vector3 startRot = transform.localEulerAngles;
        yaw = Normalize(startRot.y);
        pitch = Normalize(startRot.x);
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivityX * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivityY * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;

        yaw = Mathf.Clamp(yaw, -maxYaw, maxYaw);
        pitch = Mathf.Clamp(pitch, -maxPitchDown, maxPitchUp);

        targetRotation = Quaternion.Euler(pitch, yaw, 0f);

        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            targetRotation,
            smoothTime * Time.deltaTime
        );
    }

    float Normalize(float angle)
    {
        if (angle > 180f) angle -= 360f;
        return angle;
    }
}
