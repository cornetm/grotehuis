using UnityEngine;

public class ItemFollow : MonoBehaviour
{
    [Header("References")]
    public Transform playerCamera;          // Sleep hier je camera
    public PlayerMovement playerMovement;   // PlayerMovement script (voor crouch info)

    [Header("Objects To Follow")]
    public GameObject[] objectsToFollow;    // Sleep hier alle objecten die moeten volgen

    [Header("Follow Settings")]
    public Vector3 offset = new Vector3(0.2f, -0.2f, 0.5f); // positie t.o.v. camera
    public float positionSmooth = 8f;
    public float rotationSmooth = 12f;

    [Header("Bob Settings")]
    public float bobAmount = 0.02f;
    public float bobSpeed = 8f;

    private float bobTimer = 0f;

    void LateUpdate()
    {
        if (playerCamera == null || playerMovement == null || objectsToFollow == null)
            return;

        // ================= ROTATION =================
        Quaternion targetRotation = Quaternion.Euler(
            playerCamera.eulerAngles.x,
            playerCamera.eulerAngles.y,
            0f
        );

        // ================= POSITION =================
        Vector3 basePosition = playerCamera.position + playerCamera.TransformDirection(offset);

        // ================= ADD BOB =================
        bool moving = Mathf.Abs(Input.GetAxis("Horizontal")) > 0.01f ||
                      Mathf.Abs(Input.GetAxis("Vertical")) > 0.01f;

        if (moving && playerMovement.controller.isGrounded)
        {
            float speed = playerMovement.isSprinting ? bobSpeed * 1.5f : bobSpeed;
            bobTimer += Time.deltaTime * speed;
            basePosition += playerCamera.right * Mathf.Sin(bobTimer) * bobAmount;
            basePosition += Vector3.up * Mathf.Sin(bobTimer * 2f) * bobAmount;
        }
        else
        {
            bobTimer = 0f;
        }

        // ================= APPLY TO ALL OBJECTS =================
        foreach (GameObject obj in objectsToFollow)
        {
            if (obj == null) continue;

            // Smooth positie
            obj.transform.position = Vector3.Lerp(obj.transform.position, basePosition, Time.deltaTime * positionSmooth);

            // Smooth rotatie
            obj.transform.rotation = Quaternion.Slerp(obj.transform.rotation, targetRotation, Time.deltaTime * rotationSmooth);
        }
    }
}
