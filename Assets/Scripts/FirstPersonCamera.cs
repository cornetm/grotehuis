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

    public InventorySystem inventorySystem;

    private Highlight currentHighlight;
    private GameObject currentInteractObject;
    private GameObject currentPrefab;
    private Texture currentIcon;
    private float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (crossbarImage != null)
            crossbarImage.color = Color.red;
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
            if (crossbarImage != null)
                crossbarImage.color = Color.white;

            Highlight highlight = hit.collider.GetComponent<Highlight>();
            if (highlight != null && highlight != currentHighlight)
            {
                if (currentHighlight != null)
                    currentHighlight.DisableHighlight();

                highlight.EnableHighlight();
                currentHighlight = highlight;
                currentInteractObject = hit.collider.gameObject;

                PrefabReferenceAuto prefabRef = currentInteractObject.GetComponent<PrefabReferenceAuto>();
                if (prefabRef != null)
                {
                    currentPrefab = prefabRef.prefab;
                    currentIcon = prefabRef.icon;
                }
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
            if (inventorySystem != null && currentPrefab != null)
            {
                bool inventoryFull = inventorySystem.IsFull();
                bool hasEquipped = inventorySystem.HasEquippedItem();

                if (!inventoryFull)
                {
                    inventorySystem.AddItem(currentPrefab, currentIcon);
                    Destroy(currentInteractObject);
                    Debug.Log($"Picked up {currentPrefab.name}.");
                }
                else if (hasEquipped)
                {
                    inventorySystem.ReplaceEquippedItem(currentPrefab, currentIcon);
                    Destroy(currentInteractObject);
                    Debug.Log($"Replaced equipped item with {currentPrefab.name}.");
                }
                else
                {
                    Debug.Log("Cannot pick up item: Inventory full & no item equipped!");
                }

                ResetHighlightAndCrossbar();
            }
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
        currentPrefab = null;
        currentIcon = null;

        if (crossbarImage != null)
            crossbarImage.color = Color.red;
    }
}
