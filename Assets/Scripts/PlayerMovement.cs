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

    [HideInInspector] public CharacterController controller;
    [HideInInspector] public bool isCrouching;
    [HideInInspector] public bool isSprinting;

    float standingHeight;
    float targetHeight;
    float heightVelocity;
    float verticalVelocity;
    float currentSprint;

    // Flag om te voorkomen dat jump afgaat bij uncrouch
    private bool justUncrouched = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        standingHeight = controller.height;
        targetHeight = standingHeight;
        currentSprint = maxSprint;

        // Init beide sliders
        if (sprintSlider1)
        {
            sprintSlider1.maxValue = maxSprint;
            sprintSlider1.value = currentSprint;
        }
        if (sprintSlider2)
        {
            sprintSlider2.maxValue = maxSprint;
            sprintSlider2.value = currentSprint;
        }

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

        // Reset flag voor volgende frame
        justUncrouched = false;
    }

    // ================= CROUCH =================
    void HandleCrouch()
    {
        // Toggle crouch met Control
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isCrouching = !isCrouching;
            targetHeight = isCrouching ? crouchHeight : standingHeight;
        }

        // Spacebar in crouch → uncrouch, geen jump
        if (Input.GetButtonDown("Jump") && isCrouching)
        {
            isCrouching = false;
            targetHeight = standingHeight;
            justUncrouched = true; // voorkomt springen in dezelfde frame
        }
    }

    void SmoothCapsule()
    {
        float prevHeight = controller.height;
        controller.height = Mathf.SmoothDamp(controller.height, targetHeight, ref heightVelocity, crouchSmoothTime);
        float delta = controller.height - prevHeight;
        controller.center += Vector3.up * (delta / 2f);
    }

    // ================= MOVEMENT =================
    void HandleMovement()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        float speed = moveSpeed;

        if (isCrouching) speed *= crouchMultiplier;
        if (isSprinting) speed *= sprintMultiplier;

        if (controller.isGrounded)
        {
            if (verticalVelocity < 0) verticalVelocity = -2f;

            // Alleen springen als niet crouching en niet net uncrouched
            if (Input.GetButtonDown("Jump") && !isCrouching && !justUncrouched)
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Realistic jump/fall
        if (!controller.isGrounded)
        {
            if (verticalVelocity < 0)
                verticalVelocity += gravity * fallMultiplier * Time.deltaTime;
            else
                verticalVelocity += gravity * lowJumpMultiplier * Time.deltaTime;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        Vector3 velocity = move * speed;
        velocity.y = verticalVelocity;
        controller.Move(velocity * Time.deltaTime);
    }

    // ================= SPRINT =================
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

    // ================= UI =================
    void UpdateUI()
    {
        if (sprintSlider1)
            sprintSlider1.value = currentSprint;
        if (sprintSlider2)
            sprintSlider2.value = currentSprint;
    }
}
