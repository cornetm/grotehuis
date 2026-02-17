using UnityEngine;

public class FlashlightFollow : MonoBehaviour
{
    [Header("References")]
    public Transform playerCamera;   // Je main camera
    public PlayerMovement pm;        // PlayerMovement script voor crouch info

    [Header("Follow Settings")]
    public Vector3 offset = new Vector3(0.2f, -0.2f, 0.5f); // relative positie t.o.v. camera
    public float positionSmooth = 8f;
    public float rotationSmooth = 12f;

    [Header("Bob Settings")]
    public float bobAmount = 0.02f;
    public float bobSpeed = 8f;

    Vector3 targetPosition;
    float bobTimer = 0f;

    void LateUpdate()
    {
        if (!playerCamera || !pm) return;

        // ================= ROTATION =================
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.Euler(playerCamera.eulerAngles.x, playerCamera.eulerAngles.y, 0f),
            Time.deltaTime * rotationSmooth
        );

        // ================= POSITION =================
        targetPosition = playerCamera.position + playerCamera.TransformDirection(offset);

        // ================= ADD BOB =================
        bool moving = Mathf.Abs(Input.GetAxis("Horizontal")) > 0.01f ||
                      Mathf.Abs(Input.GetAxis("Vertical")) > 0.01f;

        if (moving && pm.controller.isGrounded)
        {
            float speed = pm.isSprinting ? bobSpeed * 1.5f : bobSpeed;
            bobTimer += Time.deltaTime * speed;
            targetPosition += playerCamera.right * Mathf.Sin(bobTimer) * bobAmount;
            targetPosition += Vector3.up * Mathf.Sin(bobTimer * 2f) * bobAmount;
        }
        else
        {
            bobTimer = 0f;
        }

        // ================= SMOOTH MOVE =================
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * positionSmooth);
    }
}
