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

    [Header("Jump / Crouch Sway Settings")]
    public float jumpSwayAmountX = 0.03f;
    public float jumpSwayAmountY = 0.05f;
    public float jumpSwaySpeed = 5f;

    public float crouchSwayAmountX = 0.03f;
    public float crouchSwayAmountY = 0.05f;
    public float crouchSwaySpeed = 7f;

    [Header("Smooth Settings")]
    public float smoothSpeed = 8f;

    [HideInInspector] public PlayerMovement playerMovement;

    private Vector3 initialLocalPosition;
    private Vector3 targetLocalPosition;

    private Vector3 bobOffset;
    private Vector3 jumpSwayOffset;
    private Vector3 crouchSwayOffset;

    private float bobTimer = 0f;
    private bool wasCrouchingLastFrame = false;
    private bool lockY = false;   // Flag voor Y lock
    private float lockedY = 0f;   // Y waarde van camera wanneer gelockt

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
        HandleJumpAndCrouchSway();
        ApplyCameraPosition();
    }

    // ================= BOBBING =================
    private void HandleBobbing()
    {
        if (playerMovement == null) return;

        bool isMoving = (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.01f ||
                         Mathf.Abs(Input.GetAxis("Vertical")) > 0.01f) &&
                        playerMovement.controller.isGrounded;

        // Stilstaand crouched → geen bobbing
        if (playerMovement.isCrouching && !isMoving)
        {
            bobOffset = Vector3.zero;
            bobTimer = 0f;
            return;
        }

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
            bobOffset = new Vector3(bobX, bobY, 0f);
        }
        else
        {
            bobOffset = Vector3.Lerp(bobOffset, Vector3.zero, Time.deltaTime * speed);
            bobTimer = 0f;
        }
    }

    // ================= JUMP & CROUCH SWAY =================
    private void HandleJumpAndCrouchSway()
    {
        if (playerMovement == null) return;

        bool isMoving = Mathf.Abs(Input.GetAxis("Horizontal")) > 0.01f || Mathf.Abs(Input.GetAxis("Vertical")) > 0.01f;

        // Jump sway
        if (!playerMovement.controller.isGrounded)
        {
            jumpSwayOffset.x = Mathf.Sin(Time.time * jumpSwaySpeed) * jumpSwayAmountX;
            jumpSwayOffset.y = Mathf.Sin(Time.time * jumpSwaySpeed * 1.2f) * jumpSwayAmountY;
        }
        else
        {
            jumpSwayOffset = Vector3.Lerp(jumpSwayOffset, Vector3.zero, Time.deltaTime * jumpSwaySpeed);
        }

        // Crouch sway
        if (playerMovement.isCrouching != wasCrouchingLastFrame)
        {
            float direction = playerMovement.isCrouching ? -1f : 1f;
            crouchSwayOffset = new Vector3(crouchSwayAmountX * direction, crouchSwayAmountY * direction, 0f);
            wasCrouchingLastFrame = playerMovement.isCrouching;

            if (playerMovement.isCrouching)
            {
                // Lock Y zodra crouch is ingeschakeld
                lockY = true;
                lockedY = playerMovement.playerCamera.localPosition.y;
            }
            else
            {
                lockY = false;
            }
        }
        else if (!playerMovement.isCrouching || isMoving)
        {
            crouchSwayOffset = Vector3.Lerp(crouchSwayOffset, Vector3.zero, Time.deltaTime * crouchSwaySpeed);
        }
        else
        {
            crouchSwayOffset = Vector3.zero;
        }
    }

    // ================= APPLY CAMERA POSITION =================
    private void ApplyCameraPosition()
    {
        targetLocalPosition = initialLocalPosition + bobOffset + jumpSwayOffset + crouchSwayOffset;

        // Als Y gelockt is, override target Y
        if (lockY)
        {
            targetLocalPosition.y = lockedY;
        }

        transform.localPosition = Vector3.Lerp(transform.localPosition, targetLocalPosition, Time.deltaTime * smoothSpeed);
    }
}
