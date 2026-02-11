using UnityEngine;
using UnityEngine.UI;

public class FirstPersonCamera : MonoBehaviour
{
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

    [Header("Raycast / Interaction Settings")]
    public float rayDistance = 5f;
    public float sphereRadius = 0.3f;
    public Image crosshairImage;
    public Color defaultCrossColor = Color.red;
    public Color highlightColor = Color.white;
    public InventorySystem inventorySystem;

    private float xRotation = 0f;
    private float eyeTimer = 0f;
    private Quaternion targetRotation;
    private float currentTilt = 0f;
    private float targetTilt = 0f;

    private PlayerMovement player;
    private Highlight currentHighlight;
    private GameObject currentInteractObject;
    private GameObject currentPrefab;
    private Texture currentIcon;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        targetRotation = transform.localRotation;

        player = playerBody.GetComponent<PlayerMovement>();
        if (player == null) Debug.LogError("PlayerMovement component not found!");

        if (crosshairImage) crosshairImage.color = defaultCrossColor;
    }

    void Update()
    {
        HandleCameraRotation();
        HandleRaycast();
        HandleInteractionInput();
    }

    // ================= CAMERA ROTATION + SWAY =================
    private void HandleCameraRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerBody.Rotate(Vector3.up * mouseX);

        bool isMoving = (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.01f ||
                         Mathf.Abs(Input.GetAxis("Vertical")) > 0.01f) &&
                        player.controller.isGrounded;

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

        // Strafing tilt
        float horizontalInput = Input.GetAxis("Horizontal");
        targetTilt = -horizontalInput * tiltAmount;
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSmooth);

        // Jump sway
        if (!player.controller.isGrounded)
        {
            float jumpTiltOffset = Mathf.Sin(Time.time * jumpSwaySpeed) * jumpTilt;
            currentTilt += jumpTiltOffset;
        }

        Quaternion finalRotation = targetRotation * Quaternion.Euler(0f, 0f, currentTilt);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, finalRotation, Time.deltaTime * rotationSmoothSpeed);
    }

    // ================= RAYCAST INTERACTION =================
    private void HandleRaycast()
    {
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;

        RaycastHit hit;
        bool hitSomething = Physics.SphereCast(origin, sphereRadius, direction, out hit, rayDistance);

        Debug.DrawRay(origin, direction * rayDistance, Color.green);

        if (hitSomething && hit.collider.CompareTag("Interaction"))
        {
            if (crosshairImage) crosshairImage.color = highlightColor;

            Highlight highlight = hit.collider.GetComponent<Highlight>();
            if (highlight && highlight != currentHighlight)
            {
                if (currentHighlight) currentHighlight.DisableHighlight();

                highlight.EnableHighlight();
                currentHighlight = highlight;
                currentInteractObject = hit.collider.gameObject;

                PrefabReferenceAuto prefabRef = currentInteractObject.GetComponent<PrefabReferenceAuto>();
                if (prefabRef)
                {
                    currentPrefab = prefabRef.prefab;
                    currentIcon = prefabRef.icon;
                }
            }
        }
        else ResetHighlightAndCrosshair();
    }

    private void ResetHighlightAndCrosshair()
    {
        if (currentHighlight) currentHighlight.DisableHighlight();
        currentHighlight = null;
        currentInteractObject = null;
        currentPrefab = null;
        currentIcon = null;

        if (crosshairImage) crosshairImage.color = defaultCrossColor;
    }

    // ================= INTERACTIE INPUT =================
    private void HandleInteractionInput()
    {
        if (currentInteractObject != null && Input.GetKeyDown(KeyCode.E))
        {
            if (inventorySystem && currentPrefab)
            {
                if (!inventorySystem.IsFull())
                {
                    inventorySystem.AddItem(currentPrefab, currentIcon);
                    Destroy(currentInteractObject);
                }
                else if (inventorySystem.HasEquippedItem())
                {
                    inventorySystem.ReplaceEquippedItem(currentPrefab, currentIcon);
                    Destroy(currentInteractObject);
                }

                ResetHighlightAndCrosshair();
            }
        }
    }
}
