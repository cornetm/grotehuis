using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float sprintMultiplier = 2f;
    public float crouchMultiplier = 0.5f;

    [Header("Jump")]
    public float jumpHeight = 3.2f;
    public float gravity = -9.81f;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 1.7f;

    [Header("Crouch")]
    public float crouchHeight = 1f;
    public float crouchSmoothTime = 0.2f;

    [Header("Sprint")]
    public float maxSprint = 5f;
    public float sprintDrain = 1f;
    public float sprintRegen = 1f;

    public Slider sprintSlider1;
    public Slider sprintSlider2;

    [Header("Outside")]
    public float outsideSpeed = 10f;

    [HideInInspector] public CharacterController controller;
    [HideInInspector] public bool isCrouching;
    [HideInInspector] public bool isSprinting;

    float targetHeight;
    float heightVelocity;
    float verticalVelocity;
    float currentSprint;

    private bool justUncrouched = false;

    // 🔥 STABLE STATE
    private bool isRunnerMode;
    private float runnerExitTimer;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        targetHeight = controller.height;
        currentSprint = maxSprint;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleCrouch();
        SmoothCapsule();
        HandleMovement();
        HandleSprint();
        UpdateUI();

        justUncrouched = false;

        if (runnerExitTimer > 0)
            runnerExitTimer -= Time.deltaTime;
        else
            isRunnerMode = false;
    }

    void HandleMovement()
    {
        Vector3 move = Vector3.zero;

        if (isRunnerMode)
        {
            // ================= RUNNER MODE =================

            float xInput = 0f;

            if (Input.GetKey(KeyCode.A))
                xInput = -1f;

            if (Input.GetKey(KeyCode.D))
                xInput = 1f;

            Vector3 forward = Vector3.forward * outsideSpeed;
            Vector3 side = Vector3.right * xInput * outsideSpeed;

            Vector3 horizontalMove = forward + side;

            // Jump logic
            if (controller.isGrounded)
            {
                if (verticalVelocity < 0)
                    verticalVelocity = -2f;

                if (Input.GetButtonDown("Jump") && !justUncrouched)
                    verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            verticalVelocity += gravity * Time.deltaTime;

            // 🔥 FIX: combine correctly
            move = horizontalMove;
            move.y = verticalVelocity;

            controller.Move(move * Time.deltaTime);
            return;
        }
        else
        {
            // ================= NORMAL MODE =================

            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            Vector3 normalMove = transform.right * x + transform.forward * z;

            float speed = moveSpeed;

            if (isCrouching) speed *= crouchMultiplier;
            if (isSprinting) speed *= sprintMultiplier;

            if (controller.isGrounded)
            {
                if (verticalVelocity < 0)
                    verticalVelocity = -2f;

                if (Input.GetButtonDown("Jump") && !isCrouching && !justUncrouched)
                    verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            if (!controller.isGrounded)
            {
                if (verticalVelocity < 0)
                    verticalVelocity += gravity * fallMultiplier * Time.deltaTime;
                else
                    verticalVelocity += gravity * lowJumpMultiplier * Time.deltaTime;
            }

            move = normalMove * speed;
        }

        move.y = verticalVelocity;
        controller.Move(move * Time.deltaTime);
    }

    void HandleCrouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.C))
        {
            isCrouching = !isCrouching;
            targetHeight = isCrouching ? crouchHeight : 1.8f;
        }

        if (Input.GetButtonDown("Jump") && isCrouching)
        {
            isCrouching = false;
            targetHeight = 1.8f;
            justUncrouched = true;
        }
    }

    void SmoothCapsule()
    {
        float prevHeight = controller.height;
        controller.height = Mathf.SmoothDamp(controller.height, targetHeight, ref heightVelocity, crouchSmoothTime);
        controller.center += Vector3.up * (controller.height - prevHeight) / 2f;
    }

    void HandleSprint()
    {
        bool holdingShift = Input.GetKey(KeyCode.LeftShift);

        if (holdingShift && currentSprint > 0 && !isCrouching)
        {
            isSprinting = true;
            currentSprint -= sprintDrain * Time.deltaTime;
        }
        else
        {
            isSprinting = false;
            currentSprint += sprintRegen * Time.deltaTime;
        }

        currentSprint = Mathf.Clamp(currentSprint, 0, maxSprint);
    }

    void UpdateUI() { }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.CompareTag("Outside"))
        {
            isRunnerMode = true;
            runnerExitTimer = 0.2f;
        }
    }
}