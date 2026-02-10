using UnityEngine;

public class CameraBobbing : MonoBehaviour
{
    [Header("Bobbing Settings")]
    public float walkBobSpeed = 14f;   // verticale snelheid bij lopen
    public float walkBobAmount = 0.05f; // verticale amplitude
    public float walkSwayAmount = 0.03f; // horizontale amplitude

    public float sprintBobSpeed = 18f;
    public float sprintBobAmount = 0.08f;
    public float sprintSwayAmount = 0.05f;

    public float crouchBobSpeed = 8f;
    public float crouchBobAmount = 0.02f;
    public float crouchSwayAmount = 0.01f;

    private float defaultYPos;
    private float defaultXPos;
    private float timer = 0f;

    private PlayerMovement playerMovement;

    void Start()
    {
        defaultYPos = transform.localPosition.y;
        defaultXPos = transform.localPosition.x;
        playerMovement = GetComponentInParent<PlayerMovement>();
        if (playerMovement == null)
            Debug.LogError("CameraBobbing: PlayerMovement not found in parent!");
    }

    void Update()
    {
        if (playerMovement == null) return;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        bool isMoving = (horizontal != 0 || vertical != 0) && playerMovement.controller.isGrounded;

        if (isMoving)
        {
            float speed = walkBobSpeed;
            float verticalAmount = walkBobAmount;
            float horizontalAmount = walkSwayAmount;

            if (playerMovement.isCrouching)
            {
                speed = crouchBobSpeed;
                verticalAmount = crouchBobAmount;
                horizontalAmount = crouchSwayAmount;
            }
            else if (playerMovement.isSprinting)
            {
                speed = sprintBobSpeed;
                verticalAmount = sprintBobAmount;
                horizontalAmount = sprintSwayAmount;
            }

            timer += Time.deltaTime * speed;

            // Vertical bob (op en neer)
            float newY = defaultYPos + Mathf.Sin(timer) * verticalAmount;

            // Horizontal sway (links-rechts)
            float newX = defaultXPos + Mathf.Sin(timer * 0.5f) * horizontalAmount;

            transform.localPosition = new Vector3(newX, newY, transform.localPosition.z);
        }
        else
        {
            // reset naar standaardpositie als de speler stilstaat
            timer = 0f;
            transform.localPosition = new Vector3(
                Mathf.Lerp(transform.localPosition.x, defaultXPos, Time.deltaTime * 5f),
                Mathf.Lerp(transform.localPosition.y, defaultYPos, Time.deltaTime * 5f),
                transform.localPosition.z
            );
        }
    }
}
