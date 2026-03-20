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

    // ================= INTERACTION =================
    public float rayDistance = 5f;
    public float sphereRadius = 0.3f;
    public float raycastEyeOffset = 0.6f;

    public InventorySystem inventorySystem;

    Highlight currentHighlight;
    GameObject currentInteractObject;
    GameObject currentPrefab;
    Texture currentIcon;

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
        HandleRaycast();
        HandleInteractionInput();
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

    // ================= FIXED RAYCAST =================
    void HandleRaycast()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 screenCenter =
            crosshairImage != null
            ? crosshairImage.rectTransform.position
            : new Vector3(Screen.width / 2f, Screen.height / 2f, 0);

        Ray ray = cam.ScreenPointToRay(screenCenter);

        Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.green);

        // ❗ BLOCKING CHECK (NIEUWE FIX)
        if (Physics.Raycast(ray, out RaycastHit blockHit, rayDistance))
        {
            if (!blockHit.collider.CompareTag("Interaction"))
            {
                ResetHighlight();
                return;
            }
        }

        RaycastHit[] hits = Physics.SphereCastAll(ray.origin, sphereRadius, ray.direction, rayDistance);

        RaycastHit? bestHit = null;
        float closest = Mathf.Infinity;

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.CompareTag("Player")) continue;
            if (!hit.collider.CompareTag("Interaction")) continue;

            if (hit.distance < closest)
            {
                closest = hit.distance;
                bestHit = hit;
            }
        }

        if (bestHit.HasValue)
        {
            RaycastHit hit = bestHit.Value;

            if (crosshairImage)
                crosshairImage.color = Color.white;

            Highlight h = hit.collider.GetComponent<Highlight>();

            if (h && h != currentHighlight)
            {
                if (currentHighlight) currentHighlight.DisableHighlight();

                h.EnableHighlight();
                currentHighlight = h;
                currentInteractObject = hit.collider.gameObject;

                PrefabReferenceAuto prefabRef =
                    currentInteractObject.GetComponent<PrefabReferenceAuto>();

                if (prefabRef)
                {
                    currentPrefab = prefabRef.prefab;
                    currentIcon = prefabRef.icon;
                }
            }
        }
        else
        {
            ResetHighlight();
        }
    }

    void ResetHighlight()
    {
        if (currentHighlight)
            currentHighlight.DisableHighlight();

        currentHighlight = null;
        currentInteractObject = null;
        currentPrefab = null;
        currentIcon = null;

        if (crosshairImage)
            crosshairImage.color = defaultCrossColor;
    }

    // ================= INTERACTION INPUT =================
    void HandleInteractionInput()
    {
        if (currentInteractObject && Input.GetKeyDown(KeyCode.E))
        {
            OpenDOor door = currentInteractObject.GetComponent<OpenDOor>();

            if (door != null)
            {
                door.ToggleDoor();
                return;
            }

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

                ResetHighlight();
            }
        }
    }
}