using UnityEngine;

public class StartDoor : MonoBehaviour
{
    [Header("Door Settings")]
    public float closedYRotation = -90f;
    public float openSpeed = 3f;

    private bool closeDoor = false;
    private Quaternion targetRotation;

    void Start()
    {
        targetRotation = transform.rotation;
    }

    void Update()
    {
        if (closeDoor)
        {
            Quaternion desiredRotation = Quaternion.Euler(
                transform.eulerAngles.x,
                closedYRotation,
                transform.eulerAngles.z
            );

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                desiredRotation,
                Time.deltaTime * openSpeed
            );
        }
    }

    // 👉 trigger door sluiting
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            closeDoor = true;
        }
    }
}