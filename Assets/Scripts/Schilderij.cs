using UnityEngine;

public class Schilderij : MonoBehaviour
{
    [Header("Follow Settings")]
    public float followDistance = 2f;
    public float followSpeed = 10f;
    public float returnSpeed = 6f;

    [Header("Auto Return")]
    public float autoReturnTime = 10f;

    [Header("Scale Settings")]
    public float scaleMultiplier = 1.5f;
    public float scaleSpeed = 6f;

    [Header("Interaction Delay")] // 🔥 NIEUW
    public float interactDelay = 1f;

    private bool isHeld = false;
    private bool isReturning = false;

    private float holdTimer = 0f;
    private float interactTimer = 0f; // 🔥 NIEUW

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;

    private Camera cam;
    private Collider col;

    void Start()
    {
        cam = Camera.main;
        col = GetComponent<Collider>();

        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalScale = transform.localScale;
    }

    void Update()
    {
        HandleInput();

        if (isHeld)
        {
            FollowCamera();

            // Scale groter
            Vector3 targetScale = originalScale * scaleMultiplier;
            transform.localScale = Vector3.Lerp(
                transform.localScale,
                targetScale,
                Time.deltaTime * scaleSpeed
            );

            holdTimer += Time.deltaTime;
            interactTimer += Time.deltaTime; // 🔥 timer loopt

            if (holdTimer >= autoReturnTime)
            {
                StartReturn();
            }
        }

        if (isReturning)
            ReturnToOriginal();
    }

    void HandleInput()
    {
        // ❌ Geen interactie tijdens return
        if (isReturning) return;

        if (IsPlayerLookingAt())
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (!isHeld)
                {
                    // Oppakken
                    isHeld = true;
                    holdTimer = 0f;
                    interactTimer = 0f; // 🔥 reset delay

                    if (col != null) col.enabled = false;
                }
                else if (isHeld && interactTimer >= interactDelay) // 🔥 delay check
                {
                    StartReturn();
                }
            }
        }

        // 🔥 Linkermuisknop ook met delay
        if (isHeld && Input.GetMouseButtonDown(0) && interactTimer >= interactDelay)
        {
            StartReturn();
        }
    }

    bool IsPlayerLookingAt()
    {
        if (cam == null) return false;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 5f))
        {
            return hit.collider.gameObject == gameObject;
        }

        return false;
    }

    void FollowCamera()
    {
        if (cam == null) return;

        Vector3 targetPos =
            cam.transform.position +
            cam.transform.forward * followDistance;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPos,
            Time.deltaTime * followSpeed
        );

        Vector3 lookDir = cam.transform.position - transform.position;

        if (lookDir.sqrMagnitude > 0.001f)
        {
            Quaternion lookRot = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                lookRot,
                Time.deltaTime * followSpeed
            );
        }
    }

    void StartReturn()
    {
        isHeld = false;
        isReturning = true;

        if (col != null) col.enabled = true;
    }

    void ReturnToOriginal()
    {
        transform.position = Vector3.Lerp(
            transform.position,
            originalPosition,
            Time.deltaTime * returnSpeed
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            originalRotation,
            Time.deltaTime * returnSpeed
        );

        transform.localScale = Vector3.Lerp(
            transform.localScale,
            originalScale,
            Time.deltaTime * scaleSpeed
        );

        if (Vector3.Distance(transform.position, originalPosition) < 0.01f)
        {
            transform.position = originalPosition;
            transform.rotation = originalRotation;
            transform.localScale = originalScale;

            isReturning = false;
            holdTimer = 0f;
            interactTimer = 0f; // 🔥 reset
        }
    }
}