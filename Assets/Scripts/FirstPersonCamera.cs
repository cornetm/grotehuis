using UnityEngine;
using UnityEngine.UI;

public class FirstPersonCamera : MonoBehaviour
{
    [Header("Camera Settings")]
    public float mouseSensitivity = 100f;
    public Transform playerBody;

    [Header("Idle Eye Sway")]
    public float eyeSwayAmount = 0.5f;
    public float eyeSwaySpeed = 1.5f;
    public float rotationSmoothSpeed = 5f;

    private float xRotation = 0f;
    private float eyeTimer = 0f;
    private Quaternion targetRotation;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        targetRotation = transform.localRotation;
    }

    void Update()
    {
        HandleCameraRotation();
    }

    private void HandleCameraRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerBody.Rotate(Vector3.up * mouseX);

        // Idle sway
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        bool isMoving = (horizontal != 0 || vertical != 0) && playerBody.GetComponent<CharacterController>().isGrounded;

        if (!isMoving)
        {
            eyeTimer += Time.deltaTime * eyeSwaySpeed;
            float swayX = Mathf.Sin(eyeTimer * 0.7f) * eyeSwayAmount;
            float swayY = Mathf.Sin(eyeTimer * 1.3f) * eyeSwayAmount;
            targetRotation = Quaternion.Euler(xRotation + swayY, swayX, 0f);
        }
        else
        {
            targetRotation = Quaternion.Euler(xRotation, 0f, 0f);
            eyeTimer = Mathf.Lerp(eyeTimer, 0f, Time.deltaTime * 2f);
        }

        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * rotationSmoothSpeed);
    }
}
