using UnityEngine;

public class ItemFollow : MonoBehaviour
{
    [Header("References")]
    public Transform playerCamera;
    public PlayerMovement playerMovement;

    [Header("Objects To Follow")]
    public GameObject[] objectsToFollow;

    [Header("Follow Settings")]
    public Vector3 offset = new Vector3(0.2f, -0.2f, 0.5f);
    public float positionSmooth = 8f;
    public float rotationSmooth = 12f;

    [Header("Bob Settings")]
    public float bobAmount = 0.02f;
    public float bobSpeed = 8f;

    private Quaternion[] originalRotations;
    private float bobTimer = 0f;

    // Voor smooth rotatie per object
    private Quaternion[] currentRotations;

    void Start()
    {
        if (objectsToFollow != null && objectsToFollow.Length > 0)
        {
            originalRotations = new Quaternion[objectsToFollow.Length];
            currentRotations = new Quaternion[objectsToFollow.Length];
            for (int i = 0; i < objectsToFollow.Length; i++)
            {
                if (objectsToFollow[i] != null)
                {
                    originalRotations[i] = objectsToFollow[i].transform.localRotation;
                    currentRotations[i] = objectsToFollow[i].transform.rotation;
                }
            }
        }
    }

    void LateUpdate()
    {
        if (playerCamera == null || playerMovement == null || objectsToFollow == null)
            return;

        // ================= POSITION =================
        Vector3 targetPosition = playerCamera.position + playerCamera.TransformDirection(offset);

        // ================= ADD BOB =================
        bool moving = Mathf.Abs(Input.GetAxis("Horizontal")) > 0.01f ||
                      Mathf.Abs(Input.GetAxis("Vertical")) > 0.01f;

        if (moving && playerMovement.controller.isGrounded)
        {
            float speed = playerMovement.isSprinting ? bobSpeed * 1.5f : bobSpeed;
            bobTimer += Time.deltaTime * speed;
            targetPosition += playerCamera.right * Mathf.Sin(bobTimer) * bobAmount;
            targetPosition += Vector3.up * Mathf.Sin(bobTimer * 2f) * bobAmount;
        }
        else
        {
            bobTimer = 0f;
        }

        // ================= APPLY TO ALL OBJECTS =================
        for (int i = 0; i < objectsToFollow.Length; i++)
        {
            GameObject obj = objectsToFollow[i];
            if (obj == null) continue;

            // Smooth positie
            obj.transform.position = Vector3.Lerp(obj.transform.position, targetPosition, Time.deltaTime * positionSmooth);

            // ================= SMOOTH ROTATION =================
            Quaternion targetRotation = Quaternion.Euler(playerCamera.eulerAngles.x, playerCamera.eulerAngles.y, 0f);
            Quaternion combinedRot = targetRotation * originalRotations[i];

            // Slerp vanaf vorige rotatie per object (smooth in alle richtingen)
            currentRotations[i] = Quaternion.Slerp(currentRotations[i], combinedRot, Time.deltaTime * rotationSmooth);
            obj.transform.rotation = currentRotations[i];
        }
    }
}
