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
        if (mainCamera == null)
        {
            mainCamera = Camera.main;

            if (mainCamera == null)
                mainCamera = FindFirstObjectByType<Camera>();
        }

        normalPainting.SetActive(true);
        scaryPainting.SetActive(false);
    }

    void Update()
    {
        if (mainCamera == null) return;

        HandleLookDetection();
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