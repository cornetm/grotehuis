using UnityEngine;
using UnityEngine.UI;

public class FirstPersonCamera : MonoBehaviour
{
    [Header("Camera Settings")]
    public float mouseSensitivity = 100f;      // muisgevoeligheid
    public Transform playerBody;               // verwijzing naar speler

    [Header("Idle Eye Sway (Stilstaan)")]
    public float eyeSwayAmount = 0.5f;        // graden
    public float eyeSwaySpeed = 1.5f;         // snelheid
    public float rotationSmoothSpeed = 5f;    // smooth overgangs snelheid

    [Header("Interaction Settings")]
    public Image crossbarImage;
    public float rayDistance = 5f;
    public float sphereRadius = 0.3f;
    public InventorySystem inventorySystem;

    private float xRotation = 0f;
    private float eyeTimer = 0f;
    private Quaternion targetRotation;

    private Highlight currentHighlight;
    private GameObject currentInteractObject;
    private GameObject currentPrefab;
    private Texture currentIcon;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        targetRotation = transform.localRotation;

        if (crossbarImage) crossbarImage.color = Color.red;
    }

    void Update()
    {
        HandleCameraRotation();
        HandleSphereCast();
        HandleInteractionInput();
    }

    private void HandleCameraRotation()
    {
        // ====== Muisinput ======
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Draai speler horizontaal
        playerBody.Rotate(Vector3.up * mouseX);

        // ====== Idle sway ======
        PlayerMovement pm = playerBody.GetComponent<PlayerMovement>();
        bool isMoving = Mathf.Abs(Input.GetAxis("Horizontal")) > 0.01f || Mathf.Abs(Input.GetAxis("Vertical")) > 0.01f;
        bool isGrounded = pm != null && pm.controller.isGrounded;

        if (!isMoving && isGrounded)
        {
            // idle sway actief
            eyeTimer += Time.deltaTime * eyeSwaySpeed;
            float swayX = Mathf.Sin(eyeTimer * 0.7f) * eyeSwayAmount;
            float swayY = Mathf.Sin(eyeTimer * 1.3f) * eyeSwayAmount;

            targetRotation = Quaternion.Euler(xRotation + swayY, swayX, 0f);
        }
        else
        {
            // player beweegt of springt: kijk recht vooruit (smooth)
            targetRotation = Quaternion.Euler(xRotation, 0f, 0f);
            eyeTimer = Mathf.Lerp(eyeTimer, 0f, Time.deltaTime * 2f);
        }

        // Smooth toepassen
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * rotationSmoothSpeed);
    }

    // ====== SphereCast voor interacties ======
    private void HandleSphereCast()
    {
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;

        if (Physics.SphereCast(origin, sphereRadius, direction, out RaycastHit hit, rayDistance))
        {
            if (hit.collider.CompareTag("Interaction"))
            {
                if (crossbarImage) crossbarImage.color = Color.white;

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
        }
        else ResetHighlightAndCrossbar();
    }

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

                ResetHighlightAndCrossbar();
            }
        }
    }

    private void ResetHighlightAndCrossbar()
    {
        if (currentHighlight) currentHighlight.DisableHighlight();
        currentHighlight = null;
        currentInteractObject = null;
        currentPrefab = null;
        currentIcon = null;

        if (crossbarImage) crossbarImage.color = Color.red;
    }
}
