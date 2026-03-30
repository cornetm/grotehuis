using UnityEngine;

public class PortraitMonster : MonoBehaviour
{
    [Header("Painting Objects")]
    public GameObject normalPainting;
    public GameObject scaryPainting;

    [Header("Camera Reference")]
    public Camera mainCamera;

    [Header("Look Settings")]
    public float lookTimeToTrigger = 1f;
    public float maxDistance = 6f;

    [Header("View Tuning")]
    public float viewportMargin = 0.1f;

    private float lookTimer;
    private bool isLooking;
    private bool triggeredSwap;
    private bool isScaryActive;

    void Start()
    {
        FindCamera(); // 🔥 meteen proberen

        normalPainting.SetActive(true);
        scaryPainting.SetActive(false);
    }

    void Update()
    {
        // 🔥 blijft zoeken als camera nog niet gevonden is
        if (mainCamera == null)
        {
            FindCamera();
            return;
        }

        HandleLookDetection();
    }

    // 🔥 NIEUWE METHOD
    void FindCamera()
    {
        if (mainCamera != null) return;

        // 1. Jouw FPS camera (beste optie)
        if (FirstPersonCamera.Instance != null)
        {
            mainCamera = FirstPersonCamera.Instance.GetComponent<Camera>();
            if (mainCamera != null) return;
        }

        // 2. MainCamera tag
        mainCamera = Camera.main;
        if (mainCamera != null) return;

        // 3. Fallback
        mainCamera = FindFirstObjectByType<Camera>();
    }

    void HandleLookDetection()
    {
        Vector3 vp = mainCamera.WorldToViewportPoint(transform.position);

        bool inView =
            vp.z > 0 &&
            vp.x > -viewportMargin && vp.x < 1 + viewportMargin &&
            vp.y > -viewportMargin && vp.y < 1 + viewportMargin;

        float distance = Vector3.Distance(mainCamera.transform.position, transform.position);

        bool isLookingAtPainting = inView && distance <= maxDistance;

        if (isLookingAtPainting)
        {
            isLooking = true;
            lookTimer += Time.deltaTime;

            if (lookTimer >= lookTimeToTrigger)
            {
                triggeredSwap = true;
            }
        }
        else
        {
            if (isLooking)
            {
                isLooking = false;
                lookTimer = 0f;

                if (triggeredSwap && !isScaryActive)
                {
                    SwapToScary();
                    triggeredSwap = false;
                }
            }
        }
    }

    void SwapToScary()
    {
        normalPainting.SetActive(false);
        scaryPainting.SetActive(true);

        isScaryActive = true;
    }
}