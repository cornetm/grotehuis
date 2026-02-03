using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform playerBody; // Wijs hier je parent "Player" object aan

    private float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Verticale rotatie (op/af)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Horizontale rotatie (links/rechts)
        if (playerBody != null)
            playerBody.Rotate(Vector3.up * mouseX);
    }
}
