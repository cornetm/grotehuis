using UnityEngine;

public class CameraBobbing : MonoBehaviour
{
    [Header("Bobbing")]
    public float walkBobSpeed = 14f;
    public float walkBobAmount = 0.05f;

    public float sprintBobSpeed = 18f;
    public float sprintBobAmount = 0.08f;

    public float crouchBobSpeed = 8f;
    public float crouchBobAmount = 0.02f;

    [Header("Crouch Sway")]
    public float crouchCameraOffset = -0.5f;
    public float crouchSideSway = 0.03f;     // side sway tijdens crouch lopen
    public float crouchToggleSway = 0.06f;   // bij crouch toggle
    public float smoothSpeed = 8f;

    [Header("Jump Sway")]
    public float jumpToggleSway = 0.06f;     // side sway bij springen
    public float jumpTweenDuration = 0.3f;

    PlayerMovement pm;
    Vector3 startPos;
    float bobTimer;

    // Voor crouch toggle
    private bool crouchJustToggled = false;
    private float crouchTweenTimer = 0f;
    private float crouchTweenDuration = 0.3f;

    // Voor jump toggle
    private bool wasGrounded = true;
    private float jumpTweenTimer = 0f;

    void Start()
    {
        startPos = transform.localPosition;
        pm = GetComponentInParent<PlayerMovement>();
    }

    void Update()
    {
        if (!pm) return;

        bool moving = Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f ||
                      Mathf.Abs(Input.GetAxis("Vertical")) > 0.1f;

        // ================= BOB =================
        float speed = walkBobSpeed;
        float amountY = walkBobAmount;

        if (pm.isCrouching)
        {
            speed = crouchBobSpeed;
            amountY = crouchBobAmount;
        }
        else if (pm.isSprinting)
        {
            speed = sprintBobSpeed;
            amountY = sprintBobAmount;
        }

        float bobY = 0f;
        float bobX = 0f;

        if (moving && pm.controller.isGrounded)
        {
            bobTimer += Time.deltaTime * speed;
            bobY = Mathf.Sin(bobTimer) * amountY;

            if (pm.isCrouching)
                bobX = Mathf.Sin(bobTimer * 0.5f) * crouchSideSway;
        }
        else bobTimer = 0f;

        // ================= CROUCH TOGGLE SWAY =================
        if (pm.isCrouching != crouchJustToggled)
        {
            crouchJustToggled = pm.isCrouching;
            crouchTweenTimer = 0f;
        }

        float toggleCrouchOffsetX = 0f;
        if (crouchTweenTimer < crouchTweenDuration)
        {
            crouchTweenTimer += Time.deltaTime;
            float t = crouchTweenTimer / crouchTweenDuration;
            toggleCrouchOffsetX = Mathf.Sin(t * Mathf.PI) * crouchToggleSway;
        }

        // ================= JUMP TOGGLE SWAY =================
        if (!wasGrounded && pm.controller.isGrounded)
        {
            // net geland → trigger sway
            jumpTweenTimer = 0f;
        }
        else if (wasGrounded && !pm.controller.isGrounded)
        {
            // net gesprongen → trigger sway
            jumpTweenTimer = 0f;
        }

        wasGrounded = pm.controller.isGrounded;

        float toggleJumpOffsetX = 0f;
        if (jumpTweenTimer < jumpTweenDuration)
        {
            jumpTweenTimer += Time.deltaTime;
            float t = jumpTweenTimer / jumpTweenDuration;
            toggleJumpOffsetX = Mathf.Sin(t * Mathf.PI) * jumpToggleSway;
        }

        // ================= COMBINE =================
        float crouchOffsetY = pm.isCrouching ? crouchCameraOffset : 0f;

        Vector3 target =
            startPos +
            Vector3.up * (crouchOffsetY + bobY) +
            Vector3.right * (bobX + toggleCrouchOffsetX + toggleJumpOffsetX);

        transform.localPosition =
            Vector3.Lerp(transform.localPosition, target, Time.deltaTime * smoothSpeed);
    }
}
