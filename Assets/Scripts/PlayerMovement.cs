using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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

    [Header("Runner Jump")]
    public float runnerJumpMultiplier = 1.3f;

    [Header("Crouch")]
    public float crouchHeight = 1f;
    public float crouchSmoothTime = 0.2f;

    [Header("Sprint")]
    public float maxSprint = 5f;
    public float sprintDrain = 1f;
    public float sprintRegen = 1f;
    public float sprintRegenDelay = 1f; // 1 seconde cooldown voor regen

    public Slider sprintSlider1;
    public Slider sprintSlider2;

    [Header("Outside")]
    public float outsideSpeed = 10f;

    [Header("Runner Strafing")]
    public float runnerStrafeSpeedMultiplier = 0.6f;

    [Header("Camera")]
    public Transform cameraTransform;

    [Header("Start Object")]
    public GameObject startObject;

    [Header("Delete Object")]
    public GameObject deleteObject;

    [Header("Objects to Remove on Delete")]
    public List<GameObject> objectsToDisable = new List<GameObject>();

    [Header("Objects to Enable on Delete")]
    public List<GameObject> objectsToEnable = new List<GameObject>();

    [Header("Room Generator")]
    public RoomGenerator roomGenerator;

    [Header("Start Door")]
    public StartDoor startDoor;

    [Header("Start Light")]
    public Light startLight;

    [HideInInspector] public CharacterController controller;
    [HideInInspector] public bool isCrouching;
    [HideInInspector] public bool isSprinting;

    float targetHeight;
    float heightVelocity;

    float verticalVelocity;
    float currentSprint;

    private bool justUncrouched = false;
    public bool isRunnerMode = true;

    [Header("Start Delay")]
    public float startDelay = 3f;
    private bool canMove = false;

    private bool triggeredDelete = false;
    private bool triggeredStart = false;

    // ✅ Nieuwe static bool zodat meshrenderer weet wanneer te activeren
    public static bool GameStarted = false;

    // =========================
    // Sprint cooldown variables
    private float sprintCooldownTimer = 0f;
    private bool sprintingRecentlyStopped = false;
    // =========================

    void Start()
    {
        controller = GetComponent<CharacterController>();
        targetHeight = controller.height;
        currentSprint = maxSprint;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Invoke(nameof(EnableMovement), startDelay);
    }

    void EnableMovement()
    {
        canMove = true;
    }

    void Update()
    {
        if (!canMove) return;

        HandleCrouch();
        SmoothCapsule();
        HandleMovement();
        HandleSprint();
        UpdateUI();

        justUncrouched = false;
    }

    void HandleMovement()
    {
        Vector3 horizontalMove = Vector3.zero;

        // ================= RUNNER MODE =================
        if (isRunnerMode)
        {
            float xInput = 0f;
            if (Input.GetKey(KeyCode.A)) xInput = -1f;
            if (Input.GetKey(KeyCode.D)) xInput = 1f;

            Vector3 forward = cameraTransform.forward;
            forward.y = 0;
            forward.Normalize();

            Vector3 right = cameraTransform.right;
            right.y = 0;
            right.Normalize();

            horizontalMove =
                forward * outsideSpeed +
                right * xInput * outsideSpeed * runnerStrafeSpeedMultiplier;
        }
        else
        {
            // ================= NORMAL MODE =================
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            Vector3 forward = cameraTransform.forward;
            forward.y = 0;
            forward.Normalize();

            Vector3 right = cameraTransform.right;
            right.y = 0;
            right.Normalize();

            Vector3 normalMove = forward * z + right * x;

            float speed = moveSpeed;

            if (isCrouching) speed *= crouchMultiplier;
            if (isSprinting) speed *= sprintMultiplier;

            horizontalMove = normalMove * speed;
        }

        // ================= JUMP + GRAVITY =================
        if (controller.isGrounded)
        {
            if (verticalVelocity < 0)
                verticalVelocity = -2f;

            bool canJump = !isCrouching && !justUncrouched;

            if (Input.GetButtonDown("Jump") && canJump)
            {
                float finalJumpHeight = jumpHeight;

                if (isRunnerMode)
                    finalJumpHeight *= runnerJumpMultiplier;

                verticalVelocity = Mathf.Sqrt(finalJumpHeight * -2f * gravity);
            }
        }
        else
        {
            if (verticalVelocity < 0)
                verticalVelocity += gravity * fallMultiplier * Time.deltaTime;
            else
                verticalVelocity += gravity * lowJumpMultiplier * Time.deltaTime;
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 finalMove = horizontalMove;
        finalMove.y = verticalVelocity;

        controller.Move(finalMove * Time.deltaTime);
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

        // Als sprint actief is
        if (holdingShift && currentSprint > 0 && !isCrouching)
        {
            isSprinting = true;
            currentSprint -= sprintDrain * Time.deltaTime;
            sprintingRecentlyStopped = false; // reset cooldown
        }
        else
        {
            isSprinting = false;

            if (!sprintingRecentlyStopped)
            {
                sprintCooldownTimer = sprintRegenDelay; // start cooldown
                sprintingRecentlyStopped = true;
            }

            // cooldown aftellen
            if (sprintCooldownTimer > 0f)
            {
                sprintCooldownTimer -= Time.deltaTime;
            }
            else
            {
                currentSprint += sprintRegen * Time.deltaTime;
            }
        }

        currentSprint = Mathf.Clamp(currentSprint, 0, maxSprint);
    }

    void UpdateUI()
    {
        if (sprintSlider1 != null)
            sprintSlider1.value = currentSprint / maxSprint;

        if (sprintSlider2 != null)
            sprintSlider2.value = currentSprint / maxSprint;
    }

    void OnTriggerEnter(Collider other)
    {
        // ================= START OBJECT =================
        if (!triggeredStart && startObject != null && other.gameObject == startObject)
        {
            triggeredStart = true;
            isRunnerMode = false;
        }

        // ================= DELETE OBJECT =================
        if (!triggeredDelete && deleteObject != null && other.gameObject == deleteObject)
        {
            triggeredDelete = true;

            if (startDoor != null)
                startDoor.CloseDoor();

            GameStarted = true;

            float doorCloseDelay = 1.5f;
            Invoke(nameof(DeleteObjectsAndStartRoom), doorCloseDelay);
        }
    }

    void DeleteObjectsAndStartRoom()
    {
        foreach (GameObject obj in objectsToDisable)
        {
            if (obj != null)
                Destroy(obj);
        }

        foreach (GameObject obj in objectsToEnable)
        {
            if (obj != null)
                obj.SetActive(true);
        }

        if (roomGenerator != null)
            roomGenerator.BeginGeneration();

        if (startLight != null)
            startLight.intensity = 0.1f;
    }
}