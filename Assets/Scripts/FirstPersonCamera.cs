using UnityEngine;
using UnityEngine.UI;

public class FirstPersonCamera : MonoBehaviour
{
    public static FirstPersonCamera Instance;

    [Header("Camera Settings")]
    public float mouseSensitivity = 100f;
    public Transform playerBody;

    [Header("Sway Settings")]
    public float eyeSwayAmount = 0.3f;
    public float eyeSwaySpeed = 1.5f;
    public float rotationSmoothSpeed = 6f;
    public float tiltAmount = 5f;
    public float tiltSmooth = 4f;

    [Header("Jump Sway")]
    public float jumpTilt = 3f;
    public float jumpSwaySpeed = 5f;

    public Image crosshairImage;
    public Color defaultCrossColor = Color.red;

    float xRotation;
    float eyeTimer;
    float currentTilt;
    float targetTilt;

    PlayerMovement player;

    private bool isEyeContact = false;

    void Start()
    {
        Instance = this;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        player = playerBody.GetComponent<PlayerMovement>();

        if (crosshairImage)
            crosshairImage.color = defaultCrossColor;
    }

    void Update()
    {
        HandleCameraRotation();
    }

    public void SetEyeContact(bool value)
    {
        isEyeContact = value;
    }

    void HandleCameraRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerBody.Rotate(Vector3.up * mouseX);

        bool moving =
            (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.01f ||
             Mathf.Abs(Input.GetAxis("Vertical")) > 0.01f) &&
             player.controller.isGrounded;

        Quaternion targetRot;

        if (!moving && !isEyeContact)
        {
            eyeTimer += Time.deltaTime * eyeSwaySpeed;

            float swayX = Mathf.Sin(eyeTimer * 0.7f) * eyeSwayAmount;
            float swayY = Mathf.Sin(eyeTimer * 1.3f) * eyeSwayAmount;

            targetRot = Quaternion.Euler(xRotation + swayY, swayX, 0f);
        }
        else
        {
            targetRot = Quaternion.Euler(xRotation, 0f, 0f);

            if (!isEyeContact)
                eyeTimer = 0f;
        }

        float horizontal = Input.GetAxis("Horizontal");
        targetTilt = -horizontal * tiltAmount;
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSmooth);

        if (!player.controller.isGrounded)
            currentTilt += Mathf.Sin(Time.time * jumpSwaySpeed) * jumpTilt;

        transform.localRotation =
            Quaternion.Slerp(transform.localRotation,
            targetRot * Quaternion.Euler(0, 0, currentTilt),
            Time.deltaTime * rotationSmoothSpeed);
    }
}