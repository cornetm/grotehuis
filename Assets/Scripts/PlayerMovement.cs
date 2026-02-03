using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float sprintMultiplier = 2f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;

    [Header("Sprint Settings")]
    public float maxSprint = 5f;
    public float sprintDepletionRate = 1f;
    public float sprintRegenRate = 1f;
    public float sprintCooldown = 3f;

    public Slider sprintSlider1; // eerste slider
    public Slider sprintSlider2; // tweede slider

    private CharacterController controller;
    private float verticalVelocity = 0f;

    private float currentSprint;
    private float cooldownTimer = 0f;
    private bool isSprinting = false;
    private bool shiftHeldLastFrame = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        currentSprint = maxSprint;

        // Stel beide sliders in
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
    }

    void Update()
    {
        HandleMovement();
        HandleSprint();
        UpdateSliders();
    }

    // ================= MOVEMENT =================
    private void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        float speed = isSprinting ? moveSpeed * sprintMultiplier : moveSpeed;

        // Zwaartekracht & springen
        if (controller.isGrounded)
        {
            verticalVelocity = -1f;

            if (Input.GetButtonDown("Jump"))
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
}
