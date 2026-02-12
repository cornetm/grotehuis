using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float sprintMultiplier = 2f;
    public float crouchMultiplier = 0.5f;

    [Header("Jump Settings")]
    public float jumpHeight = 3.2f;
    public float gravity = -9.81f;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 1.7f;

    [Header("Crouch Settings")]
    public float crouchHeight = 1f;
    public float standingHeight;
    public bool isCrouching = false;
    public float crouchSmoothTime = 0.2f;

    [Header("Camera Settings")]
    public Transform playerCamera;
    public float cameraCrouchOffset = 0.5f;

    [Header("Sprint Settings")]
    public float maxSprint = 5f;
    public float sprintDepletionRate = 1f;
    public float sprintRegenRate = 1f;
    public float sprintCooldown = 3f;

    public Slider sprintSlider1;
    public Slider sprintSlider2;

    [Header("Audio Settings")]
    public AudioSource walkAudio;
    public float walkIntervalNormal = 0.5f;
    public float walkIntervalSprint = 0.3f;
    public float walkIntervalCrouch = 0.7f;

    [HideInInspector] public CharacterController controller;
    [HideInInspector] public float verticalVelocity;
    [HideInInspector] public bool isSprinting;

    private float walkTimer = 0f;
    private float currentSprint;
    private float cooldownTimer = 0f;
    private bool shiftHeldLastFrame = false;

    private float targetHeight;
    private float heightVelocity = 0f;

    public Vector3 initialCameraLocalPos;
    public Vector3 targetCameraLocalPos;
    public Vector3 cameraVelocity = Vector3.zero;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        standingHeight = controller.height;
        targetHeight = standingHeight;
        currentSprint = maxSprint;

        if (sprintSlider1) { sprintSlider1.maxValue = maxSprint; sprintSlider1.value = currentSprint; }
        if (sprintSlider2) { sprintSlider2.maxValue = maxSprint; sprintSlider2.value = currentSprint; }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (walkAudio) walkAudio.playOnAwake = false;

        if (playerCamera != null)
            initialCameraLocalPos = playerCamera.localPosition;

        targetCameraLocalPos = initialCameraLocalPos;
    }

    void Update()
    {
        HandleCrouch();
        SmoothCrouch();
        HandleSprint();
        HandleMovement();
        UpdateSliders();
        HandleWalkAudio();
    }

    // ================= CROUCH (TOGGLE) =================
    void HandleCrouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
        {
            isCrouching = !isCrouching;
            targetHeight = isCrouching ? crouchHeight : standingHeight;

            if (playerCamera != null)
            {
                float offset = isCrouching ? -cameraCrouchOffset : 0f;
                targetCameraLocalPos = initialCameraLocalPos + new Vector3(0, offset, 0);
            }
        }
    }

    // ================= SMOOTH CROUCH =================
    void SmoothCrouch()
    {
        bool isMoving = Mathf.Abs(Input.GetAxis("Horizontal")) > 0.01f ||
                        Mathf.Abs(Input.GetAxis("Vertical")) > 0.01f;

        float previousHeight = controller.height;

        // Lerpen als er een verschil is of als de speler beweegt
        if (Mathf.Abs(controller.height - targetHeight) > 0.01f || isMoving)
        {
            controller.height = Mathf.SmoothDamp(controller.height, targetHeight, ref heightVelocity, crouchSmoothTime);

            // Compenseer de center
            float deltaHeight = controller.height - previousHeight;
            controller.center += new Vector3(0, deltaHeight / 2f, 0);
        }

        // Camera aanpassen
        if (playerCamera != null)
        {
            if ((targetCameraLocalPos - playerCamera.localPosition).sqrMagnitude > 0.0001f)
            {
                playerCamera.localPosition = Vector3.SmoothDamp(playerCamera.localPosition, targetCameraLocalPos, ref cameraVelocity, crouchSmoothTime);
            }
        }
    }

    // ================= MOVEMENT =================
    void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 horizontalMove = transform.right * moveX + transform.forward * moveZ;
        float horizontalSpeed = moveSpeed;

        if (isCrouching) horizontalSpeed *= crouchMultiplier;
        else if (isSprinting) horizontalSpeed *= sprintMultiplier;

        if (controller.isGrounded)
        {
            if (verticalVelocity < 0f) verticalVelocity = -2f;

            if (Input.GetButtonDown("Jump") && !isCrouching)
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        else
        {
            if (verticalVelocity < 0)
                verticalVelocity += gravity * fallMultiplier * Time.deltaTime;
            else
                verticalVelocity += gravity * lowJumpMultiplier * Time.deltaTime;
        }

        Vector3 move = horizontalMove * horizontalSpeed;
        move.y = verticalVelocity;

        controller.Move(move * Time.deltaTime);
    }

    // ================= SPRINT =================
    void HandleSprint()
    {
        bool shiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (isCrouching) isSprinting = false;

        if ((shiftHeld && currentSprint <= 0f) || (!shiftHeld && shiftHeldLastFrame))
            cooldownTimer = sprintCooldown;

        shiftHeldLastFrame = shiftHeld;

        if (shiftHeld && currentSprint > 0f && !isCrouching)
        {
            isSprinting = true;
            currentSprint -= sprintDepletionRate * Time.deltaTime;
            currentSprint = Mathf.Max(currentSprint, 0f);
        }
        else
        {
            isSprinting = false;

            if (cooldownTimer > 0f)
                cooldownTimer -= Time.deltaTime;
            else if (currentSprint < maxSprint)
            {
                currentSprint += sprintRegenRate * Time.deltaTime;
                currentSprint = Mathf.Min(currentSprint, maxSprint);
            }
        }
    }

    // ================= UI =================
    void UpdateSliders()
    {
        if (sprintSlider1) sprintSlider1.value = currentSprint;
        if (sprintSlider2) sprintSlider2.value = currentSprint;
    }

    // ================= WALK AUDIO =================
    void HandleWalkAudio()
    {
        if (!walkAudio) return;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        bool isMoving = (horizontal != 0 || vertical != 0) && controller.isGrounded;

        if (isMoving)
        {
            walkTimer -= Time.deltaTime;
            float interval = isCrouching ? walkIntervalCrouch : isSprinting ? walkIntervalSprint : walkIntervalNormal;

            if (walkTimer <= 0f)
            {
                walkAudio.Play();
                walkTimer = interval;
            }
        }
        else walkTimer = 0f;
    }
}
