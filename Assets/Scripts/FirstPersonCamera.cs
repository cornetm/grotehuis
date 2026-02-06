using UnityEngine;
using UnityEngine.UI;

public class FirstPersonCamera : MonoBehaviour
{
    [Header("Camera Settings")]
    public float mouseSensitivity = 100f;
    public Transform playerBody;

    [Header("Interaction Settings")]
    public Image crossbarImage;
    public float rayDistance = 5f;
    public float sphereRadius = 0.3f;

    public InventorySystem inventorySystem; // Reference naar inventory

    private Highlight currentHighlight;
    private GameObject currentInteractObject;
    private Color originalCrossbarColor = Color.red;
    private float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (crossbarImage != null)
            originalCrossbarColor = crossbarImage.color;
    }

    void Update()
    {
        HandleCameraRotation();
        HandleSphereCast();
        HandleInteractionInput();
    }

    private void HandleCameraRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        if (playerBody != null)
            playerBody.Rotate(Vector3.up * mouseX);
    }

    private void HandleSphereCast()
    {
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;

        RaycastHit hit;
        bool hitSomething = Physics.SphereCast(origin, sphereRadius, direction, out hit, rayDistance);

        Debug.DrawRay(origin, direction * rayDistance, Color.green);

        if (hitSomething && hit.collider.CompareTag("Interaction"))
        {
            // Crossbar wit
            if (crossbarImage != null)
                crossbarImage.color = Color.white;

            // Highlight inschakelen
            Highlight highlight = hit.collider.GetComponent<Highlight>();
            if (highlight != null && highlight != currentHighlight)
            {
                if (currentHighlight != null)
                    currentHighlight.DisableHighlight();

                highlight.EnableHighlight();
                currentHighlight = highlight;
                currentInteractObject = hit.collider.gameObject;
            }
        }
        else
        {
            ResetHighlightAndCrossbar();
        }
    }

    private void HandleInteractionInput()
    {
        if (currentInteractObject != null && Input.GetKeyDown(KeyCode.E))
        {
            // Voeg prefab naam automatisch toe aan inventory
            if (inventorySystem != null)
                inventorySystem.AddItem(currentInteractObject.name);

            // Verwijder het object uit de scene
            Destroy(currentInteractObject);

            ResetHighlightAndCrossbar();
        }
    }

    private void ResetHighlightAndCrossbar()
    {
        if (currentHighlight != null)
        {
            currentHighlight.DisableHighlight();
            currentHighlight = null;
        }

        currentInteractObject = null;

        if (crossbarImage != null)
            crossbarImage.color = originalCrossbarColor;
    }
}
