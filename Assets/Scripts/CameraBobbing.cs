using UnityEngine;

public class CameraBobbing : MonoBehaviour
{
    [Header("Bobbing Settings")]
    public float walkBobSpeed = 14f;
    public float walkBobAmountY = 0.05f;
    public float walkBobAmountX = 0.05f;

    public float sprintBobSpeed = 18f;
    public float sprintBobAmountY = 0.08f;
    public float sprintBobAmountX = 0.08f;

    public float crouchBobSpeed = 8f;
    public float crouchBobAmountY = 0.02f;
    public float crouchBobAmountX = 0.02f;

    [Header("Jump / Landing Sway")]
    public float jumpSwayAmountX = 0.03f;  // horizontaal tijdens springen
    public float jumpSwayAmountY = 0.05f;  // verticaal tijdens springen
    public float jumpSwaySpeed = 5f;       // snelheid van sway in lucht

    [HideInInspector] public PlayerMovement playerMovement;

    private Vector3 initialLocalPosition;
    private Vector3 targetLocalPosition;
    private Vector3 jumpSwayOffset;
    private float bobTimer = 0f;

    void Start()
    {
        initialLocalPosition = transform.localPosition;
        targetLocalPosition = initialLocalPosition;

        playerMovement = GetComponentInParent<PlayerMovement>();
        if (playerMovement == null)
            Debug.LogError("CameraBobbing: PlayerMovement not found in parent!");
    }

    void Update()
    {
        HandleBobbing();
    }

    private void HandleBobbing()
    {
        if (playerMovement == null) return;

        bool isMoving = (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.01f ||
                         Mathf.Abs(Input.GetAxis("Vertical")) > 0.01f) &&
                        playerMovement.controller.isGrounded;

        // ===== Walk / Sprint / Crouch Bobbing =====
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

        if (isMoving)
        {
            bobTimer += Time.deltaTime * speed;
            float bobY = Mathf.Sin(bobTimer) * amountY;
            float bobX = Mathf.Sin(bobTimer * 0.5f) * amountX;
            targetLocalPosition = initialLocalPosition + new Vector3(bobX, bobY, 0f);
        }
        else
        {
            targetLocalPosition = initialLocalPosition;
            bobTimer = 0f;
        }

        // ===== Jump Sway =====
        if (!playerMovement.controller.isGrounded)
        {
            jumpSwayOffset.x = Mathf.Sin(Time.time * jumpSwaySpeed) * jumpSwayAmountX;
            jumpSwayOffset.y = Mathf.Sin(Time.time * jumpSwaySpeed * 1.2f) * jumpSwayAmountY;
        }
        else
        {
            jumpSwayOffset = Vector3.Lerp(jumpSwayOffset, Vector3.zero, Time.deltaTime * jumpSwaySpeed);
        }

        // ===== Combineer alles =====
        Vector3 finalPosition = targetLocalPosition + jumpSwayOffset;

        // Smooth transition
        transform.localPosition = Vector3.Lerp(transform.localPosition, finalPosition, Time.deltaTime * 8f);
    }
}
