using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float sprintMultiplier = 2f;
    public float crouchMultiplier = 0.5f; // snelheid bij crouch
    public float jumpHeight = 2f;
    public float gravity = -9.81f;

    [Header("Crouch Settings")]
    public float crouchHeight = 1f;
    public float standingHeight;
    public bool isCrouching = false;

    [Header("Sprint Settings")]
    public float maxSprint = 5f;
    public float sprintDepletionRate = 1f;
    public float sprintRegenRate = 1f;
    public float sprintCooldown = 3f;

    public Slider sprintSlider1;
    public Slider sprintSlider2;

    [Header("Audio Settings")]
    public AudioSource walkAudio;           // …Èn AudioSource voor alle voetstappen
    public float walkIntervalNormal = 0.5f; // Interval bij normaal lopen
    public float walkIntervalSprint = 0.3f; // Interval bij sprinten
    public float walkIntervalCrouch = 0.7f; // Interval bij crouchen

    public float walkTimer = 0f;

    public CharacterController controller;
    public float verticalVelocity = 0f;

    public float currentSprint;
    public float cooldownTimer = 0f;
    public bool isSprinting = false;
    public bool shiftHeldLastFrame = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        standingHeight = controller.height;
        currentSprint = maxSprint;

        if (sprintSlider1 != null)
        {
            sprintSlider1.maxValue = maxSprint;
            sprintSlider1.value = currentSprint;
        }
        if (sprintSlider2 != null)
        {
            sprintSlider2.maxValue = maxSprint;
            sprintSlider2.value = currentSprint;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (walkAudio != null)
            walkAudio.playOnAwake = false; // niet automatisch afspelen
    }

    void Update()
    {
        HandleCrouch();
        HandleMovement();
        HandleSprint();
        UpdateSliders();
        HandleWalkAudio();
    }

    // ================= CROUCH =================
    private void HandleCrouch()
    {
        bool ctrlHeld = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

        if (ctrlHeld)
        {
            isCrouching = true;
            controller.height = crouchHeight;
        }
        else
        {
            isCrouching = false;
            controller.height = standingHeight;
        }
    }

    // ================= MOVEMENT =================
    private void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        float speed = moveSpeed;

        if (isCrouching)
            speed *= crouchMultiplier;
        else if (isSprinting)
            speed *= sprintMultiplier;

        if (controller.isGrounded)
        {
            verticalVelocity = -1f;

            if (Input.GetButtonDown("Jump") && !isCrouching)
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        move.y = verticalVelocity;
        controller.Move(move * speed * Time.deltaTime);
    }

    // ================= SPRINT =================
    private void HandleSprint()
    {
        bool shiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (isCrouching)
        {
            isSprinting = false;
            return;
        }

        if ((shiftHeld && currentSprint <= 0f) || (!shiftHeld && shiftHeldLastFrame))
            cooldownTimer = sprintCooldown;

        shiftHeldLastFrame = shiftHeld;

        if (shiftHeld && currentSprint > 0f)
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
    private void UpdateSliders()
    {
        if (sprintSlider1 != null)
            sprintSlider1.value = currentSprint;
        if (sprintSlider2 != null)
            sprintSlider2.value = currentSprint;
    }

    // ================= WALK AUDIO =================
    private void HandleWalkAudio()
    {
        if (walkAudio == null) return;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        bool isMoving = (horizontal != 0 || vertical != 0) && controller.isGrounded;

        if (isMoving)
        {
            walkTimer -= Time.deltaTime;

            // bepaal interval afhankelijk van crouch, sprint of normaal
            float interval;
            if (isCrouching)
                interval = walkIntervalCrouch;
            else if (isSprinting)
                interval = walkIntervalSprint;
            else
                interval = walkIntervalNormal;

            if (walkTimer <= 0f)
            {
                walkAudio.Play();
                walkTimer = interval;
            }
        }
        else
        {
            walkTimer = 0f; // reset timer als je stopt
        }
    }
}
