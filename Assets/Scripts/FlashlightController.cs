using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    [Header("Flashlight Object")]
    public GameObject flashlightObject; // sleep hier het hele prefab/object in

    private bool isOn = false;

    void Start()
    {
        if (flashlightObject == null)
            flashlightObject = gameObject; // fallback: zelf het object

        SetState(false);
    }

    public void Toggle()
    {
        isOn = !isOn;
        SetState(isOn);
    }

    public void SetState(bool state)
    {
        isOn = state;

        if (flashlightObject != null)
            flashlightObject.SetActive(state);
    }
}
