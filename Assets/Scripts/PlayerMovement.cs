using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float sprintMultiplier = 2f;
    public float jumpHeight = 2f;

    [Header("Sprint Settings")]
    public float maxSprint = 5f;
    public float sprintDepletionRate = 1f;
    public float sprintRegenRate = 1f;

    [Header("Gravity Settings")]
    public float gravity = -9.81f;
    private float verticalVelocity = 0f;

    private CharacterController controller;
    private float currentSprint;
    private bool isSprinting = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        currentSprint = maxSprint;

        // Cursor verbergen en vergrendelen bij start
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMovement();
        HandleSprint();
    }

    // ================= MOVEMENT =================
    private void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        float speed = isSprinting ? moveSpeed * sprintMultiplier : moveSpeed;

        // Zwaartekracht
        if (controller.isGrounded)
        {
            verticalVelocity = -1f;

            // Springen
            if (Input.GetButtonDown("Jump"))
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
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

        if (shiftHeld && currentSprint > 0f)
        {
            isSprinting = true;
            currentSprint -= sprintDepletionRate * Time.deltaTime;
            currentSprint = Mathf.Max(currentSprint, 0f);
        }
        else
        {
            isSprinting = false;

            if (currentSprint < maxSprint)
            {
                currentSprint += sprintRegenRate * Time.deltaTime;
                currentSprint = Mathf.Min(currentSprint, maxSprint);
            }
        }
    }
}
