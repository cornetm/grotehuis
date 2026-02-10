using UnityEngine;

public class CameraBobbing : MonoBehaviour
{
    [Header("Bobbing Settings")]
    public float walkBobSpeed = 14f;
    public float walkBobAmountY = 0.05f;
    public float walkBobAmountX = 0.05f; // links/rechts beweging
    public float sprintBobSpeed = 18f;
    public float sprintBobAmountY = 0.08f;
    public float sprintBobAmountX = 0.08f;
    public float crouchBobSpeed = 8f;
    public float crouchBobAmountY = 0.02f;
    public float crouchBobAmountX = 0.02f;

    [HideInInspector]
    public PlayerMovement playerMovement;

    private float bobTimer = 0f;
    private Vector3 targetLocalPosition;
    private Vector3 initialLocalPosition;

    void Start()
    {
        initialLocalPosition = transform.localPosition;

        playerMovement = GetComponentInParent<PlayerMovement>();
        if (playerMovement == null)
            Debug.LogError("CameraBobbing: PlayerMovement not found in parent!");

        targetLocalPosition = initialLocalPosition;
    }

    void Update()
    {
        HandleBobbing();
    }

    private void HandleBobbing()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        bool isMoving = (horizontal != 0 || vertical != 0) && playerMovement.controller.isGrounded;

        if (isMoving)
        {
            float speed = walkBobSpeed;
            float amountY = walkBobAmountY;
            float amountX = walkBobAmountX;

            if (playerMovement.isCrouching)
            {
                speed = crouchBobSpeed;
                amountY = crouchBobAmountY;
                amountX = crouchBobAmountX;
            }
            else if (playerMovement.isSprinting)
            {
                speed = sprintBobSpeed;
                amountY = sprintBobAmountY;
                amountX = sprintBobAmountX;
            }

            bobTimer += Time.deltaTime * speed;
            float bobY = Mathf.Sin(bobTimer) * amountY;
            float bobX = Mathf.Sin(bobTimer * 0.5f) * amountX;

            targetLocalPosition = initialLocalPosition + new Vector3(bobX, bobY, 0f);
        }
        else
        {
            // idle, terug naar neutrale positie (blended door FirstPersonCamera)
            targetLocalPosition = initialLocalPosition;
            bobTimer = 0f;
        }

        // smooth position transition (blend)
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetLocalPosition, Time.deltaTime * 8f);
    }
}
