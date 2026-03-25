using UnityEngine;

public class HandleGame : MonoBehaviour
{
    public enum HandleType
    {
        Handle1,
        Handle2,
        Handle3
    }

    [Header("Handle Settings")]
    public HandleType handleType;

    [Header("Smooth Rotation")]
    public float rotateSpeed = 5f;

    private bool isUsed = false;

    private Quaternion startRotation;
    private Quaternion targetRotation;

    void Start()
    {
        startRotation = transform.localRotation;

        // 180 graden omlaag op X-as
        targetRotation = startRotation * Quaternion.Euler(0f, 180f, 0f);
    }

    void Update()
    {
        if (!isUsed)
        {
            if (IsPlayerLookingAtHandle() && Input.GetKeyDown(KeyCode.E))
            {
                isUsed = true;

                // collider uit zodat geen interactie meer mogelijk is
                Collider col = GetComponent<Collider>();
                if (col != null)
                    col.enabled = false;
            }
        }

        // Smooth animatie (ook nadat hij geactiveerd is)
        if (isUsed)
        {
            transform.localRotation = Quaternion.Slerp(
                transform.localRotation,
                targetRotation,
                Time.deltaTime * rotateSpeed
            );
        }
    }

    bool IsPlayerLookingAtHandle()
    {
        Camera cam = Camera.main;
        if (cam == null) return false;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 5f))
        {
            return hit.collider.gameObject == gameObject;
        }

        return false;
    }
}